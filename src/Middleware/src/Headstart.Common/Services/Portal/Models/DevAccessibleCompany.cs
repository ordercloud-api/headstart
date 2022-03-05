using OrderCloud.SDK;
using System.Collections.Generic;

namespace Headstart.Common.Services.Portal.Models
{
    public class DevAccessibleCompany
    {
        public string AdminCompanyID { get; set; } = string.Empty;

        public string AdminCompanyName { get; set; } = string.Empty;

        public bool ApiClientAdmin { get; set; }

        public bool BuyerImpersonation { get; set; }

        public bool DevAccessAdmin { get; set; }

        public string DevEmail { get; set; } = string.Empty;

        public int DevID { get; set; }

        public string DevUserName { get; set; } = string.Empty;

        public int ID { get; set; }

        public bool IntegrationProxyAdmin { get; set; }

        public bool MessageConfigAdmin { get; set; }

        public int OwnerDevID { get; set; }

        public string OwnerEmail { get; set; } = string.Empty;

        public string OwnerUserName { get; set; } = string.Empty;

        public List<ApiRole> Roles { get; set; } = new List<ApiRole>();

        public bool SecurityProfileAdmin { get; set; }

        public bool WebHookAdmin { get; set; }
    }
}