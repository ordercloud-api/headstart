using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Linq;
using Headstart.Common.Models;
using OrderCloud.SDK;
using ordercloud.integrations.library;
using Headstart.Common;

namespace Headstart.API.Commands
{
    public class DownloadReportCommand
    {
        private readonly OrderCloudIntegrationsBlobService _blob;

        public DownloadReportCommand(AppSettings settings)
        {
            _blob = new OrderCloudIntegrationsBlobService(new BlobServiceConfig()
            {
                ConnectionString = settings.StorageAccountSettings.ConnectionString,
                Container = "downloads",
                AccessType = BlobContainerPublicAccessType.Off
            });
        }

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
