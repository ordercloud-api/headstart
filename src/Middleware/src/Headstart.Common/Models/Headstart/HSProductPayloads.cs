using Headstart.Models;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace Headstart.Models.Headstart
{
    public class HSProductCreatePayload : WebhookPayloads.Products.Create<dynamic, HSProduct> { }
}
