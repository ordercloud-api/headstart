using System;
using System.Collections.Generic;
using System.Text;

namespace Headstart.Common.Models.Headstart
{
    public class AppConfigurations
    {

        public class BuyerAppConfiguration
        {
            public bool hostedApp { get; set; }
            public string appname { get; set; }
            public string appID { get; set; }
            public string clientID { get; set; }
            public string incrementorPrefix { get; set; }
            public string middlewareUrl { get; set; }
            public string creditCardIframeUrl { get; set; }
            public string translateBlobUrl { get; set; }
            public string sellerID { get; set; }
            public string sellerName { get; set; }
            public string orderCloudApiUrl { get; set; }
            public BuyerAppTheme theme { get; set; }
            public string instrumentationKey { get; set; }
        }

        public class BuyerAppTheme
        {
            public string logoSrc { get; set; }
        }
    }
}
