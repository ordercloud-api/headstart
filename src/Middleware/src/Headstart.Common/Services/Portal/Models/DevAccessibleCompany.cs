using System.Collections.Generic;
using OrderCloud.SDK;

namespace Headstart.Common.Services.Portal.Models
{
    public class DevAccessibleCompany
    {
        public string AdminCompanyID { get; set; }
        public string AdminCompanyName { get; set; }
        public bool ApiClientAdmin { get; set; }
        public bool BuyerImpersonation { get; set; }
        public bool DevAccessAdmin { get; set; }
        public string DevEmail { get; set; }
        public int DevID { get; set; }
        public string DevUserName { get; set; }
        public int ID { get; set; }
        public bool IntegrationProxyAdmin { get; set; }
        public bool MessageConfigAdmin { get; set; }
        public int OwnerDevID { get; set; }
        public string OwnerEmail { get; set; }
        public string OwnerUserName { get; set; }
        public List<ApiRole> Roles { get; set; }
        public bool SecurityProfileAdmin { get; set; }
        public bool WebHookAdmin { get; set; }
    }
}
