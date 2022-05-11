﻿using Headstart.Common;
using Headstart.Common.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json.Linq;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using Sitecore.Diagnostics;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
					Container = "downloads",
					AccessType = BlobContainerPublicAccessType.Off
				});
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
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
			var headers = reportHeaders.ToArray();
			var excel = new XSSFWorkbook();
			var worksheet = excel.CreateSheet(reportType.ToString());
			var date = DateTime.UtcNow.ToString("MMddyyyy");
			var time = DateTime.Now.ToString("hmmss.ffff");
			var fileName = $"{reportType}-{date}-{time}.xlsx";

			var fileReference = await _blob.GetAppendBlobReference(fileName);
			SetHeaders(headers, worksheet);
			SetValues(data, headers, worksheet);
			using (Stream stream = await fileReference.OpenWriteAsync(true))
			{
				excel.Write(stream);
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

		/// <summary>
		/// Private re-usable SetValues method
		/// </summary>
		/// <param name="data"></param>
		/// <param name="headers"></param>
		/// <param name="worksheet"></param>
		private void SetValues(IEnumerable<object> data, string[] headers, ISheet worksheet)
		{
			int i = 1;
			foreach (var item in data)
			{
				var dataJSON = JObject.FromObject(item);
				IRow sheetRow = worksheet.CreateRow(i++);
				int j = 0;
				foreach (var header in headers)
				{
					ICell cell = sheetRow.CreateCell(j++);
					if (header.Contains("."))
					{
						if (dataJSON[header.Split(".")[0]].ToString() != "")
						{
							var split = header.Split(".");
							var dataValue = dataJSON;
							bool hasProp = true;
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
						if (header == "Status")
						{
							cell.SetCellValue(Enum.GetName(typeof(OrderStatus), Convert.ToInt32(dataJSON[header])));
						}
						else
						{
							cell.SetCellValue(dataJSON[header].ToString());
						}
					}
				}
			}
		}

		/// <summary>
		/// Private re-usable GetSharedAccessSignature task method
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns>The SharedAccessSignature string value for the GetSharedAccessSignature process</returns>
		public async Task<string> GetSharedAccessSignature(string fileName)
		{
			var fileReference = await _blob.GetBlobReference(fileName);
			var sharedAccessPolicy = new SharedAccessBlobPolicy()
			{
				SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-5),
				SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddMinutes(20),
				Permissions = SharedAccessBlobPermissions.Read
			};
			return fileReference.GetSharedAccessSignature(sharedAccessPolicy);
		}
	}
}