using System;
using System.Linq;
using OrderCloud.SDK;
using Headstart.Common;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Newtonsoft.Json.Linq;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using Headstart.Common.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ordercloud.integrations.library;
using Microsoft.WindowsAzure.Storage.Blob;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.API.Commands
{
	public class DownloadReportCommand
	{
		private readonly OrderCloudIntegrationsBlobService _blob;
		private readonly AppSettings _settings;

		/// <summary>
		/// The IOC based constructor method for the DownloadReportCommand class object with Dependency Injection
		/// </summary>
		/// <param name="settings"></param>
		public DownloadReportCommand(AppSettings settings)
		{
			try
			{
				_settings = settings;
				_blob = new OrderCloudIntegrationsBlobService(new BlobServiceConfig()
				{
					ConnectionString = settings.StorageAccountSettings.ConnectionString,
					Container = @"downloads",
					AccessType = BlobContainerPublicAccessType.Off
				});
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable ExportToExcel task method
		/// </summary>
		/// <param name="reportType"></param>
		/// <param name="reportHeaders"></param>
		/// <param name="data"></param>
		/// <returns>The fileName string value for the ExportToExcel process</returns>
		public async Task<string> ExportToExcel(ReportTypeEnum reportType, List<string> reportHeaders, IEnumerable<object> data)
		{
			var fileName = string.Empty;
			try
			{
				var headers = reportHeaders.ToArray();
				var excel = new XSSFWorkbook();
				var worksheet = excel.CreateSheet(reportType.ToString());
				var date = DateTime.UtcNow.ToString(@"MMddyyyy");
				var time = DateTime.Now.ToString(@"hmmss.ffff");
				fileName = $@"{reportType}-{date}-{time}.xlsx";

				var fileReference = await _blob.GetAppendBlobReference(fileName);
				SetHeaders(headers, worksheet);
				SetValues(data, headers, worksheet);
				using (var stream = await fileReference.OpenWriteAsync(true))
				{
					excel.Write(stream);
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return fileName;
		}

		/// <summary>
		/// Private re-usable SetHeaders method
		/// </summary>
		/// <param name="headers"></param>
		/// <param name="worksheet"></param>
		private void SetHeaders(string[] headers, ISheet worksheet)
		{
			try
			{
				var header = worksheet.CreateRow(0);
				for (var i = 0; i < headers.Count(); i++)
				{
					var cell = header.CreateCell(i);
					var concatHeader = headers[i];
					if (headers[i].Contains("."))
					{
						var split = headers[i].Split('.');
						concatHeader = split[split.Length - 1];
					}
					var humanizedHeader = Regex.Replace(concatHeader, "([a-z](?=[0-9A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
					cell.SetCellValue(humanizedHeader);
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable SetValues method
		/// </summary>
		/// <param name="data"></param>
		/// <param name="headers"></param>
		/// <param name="worksheet"></param>
		private void SetValues(IEnumerable<object> data, string[] headers, ISheet worksheet)
		{
			try
			{
				var i = 1;
				foreach (var item in data)
				{
					var dataJSON = JObject.FromObject(item);
					var sheetRow = worksheet.CreateRow(i++);

					var j = 0;
					foreach (var header in headers)
					{
						var cell = sheetRow.CreateCell(j++);
						if (header.Contains("."))
						{
							if (dataJSON[header.Split(".")[0]].ToString() != "")
							{
								var split = header.Split(".");
								var dataValue = dataJSON;
								var hasProp = true;
								for (var k = 0; k < split.Length - 1; k++)
								{
									var prop = split[k];
									if (!dataValue.ContainsKey(prop))
									{
										hasProp = false;
										break;
									}

									var propValue = dataValue[prop];
									if (propValue == null || !propValue.HasValues)
									{
										hasProp = false;
										break;
									}
									dataValue = JObject.Parse(dataValue[prop].ToString());
								}
								if (hasProp && dataValue.ContainsKey(split[split.Length - 1]))
								{
									var value = dataValue.GetValue(split[split.Length - 1]);
									if (value.GetType() == typeof(JArray))
									{
										// Pulls first item from array if data is JArray type.
										// Supplier Name on Buyer Line Item Report uses this, always only one value in the array.
										cell.SetCellValue(((JArray)value).Count() > 0 ? value[0].ToString() : null);
									}
									else
									{
										cell.SetCellValue(value.ToString());
									}
								}
								else
								{
									cell.SetCellValue((string)null);
								}
							}
							else
							{
								cell.SetCellValue("");
							}
						}
						else
						{
							cell.SetCellValue(header == @"Status"
								? Enum.GetName(typeof(OrderStatus), Convert.ToInt32(dataJSON[header]))
								: dataJSON[header].ToString());
						}
					}
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable GetSharedAccessSignature task method
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns>The SharedAccessSignature string value for the GetSharedAccessSignature process</returns>
		public async Task<string> GetSharedAccessSignature(string fileName)
		{
			var resp = string.Empty;
			try
			{
				var fileReference = await _blob.GetBlobReference(fileName);
				var sharedAccessPolicy = new SharedAccessBlobPolicy()
				{
					SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-5),
					SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddMinutes(20),
					Permissions = SharedAccessBlobPermissions.Read
				};
				resp = fileReference.GetSharedAccessSignature(sharedAccessPolicy);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}
	}
}