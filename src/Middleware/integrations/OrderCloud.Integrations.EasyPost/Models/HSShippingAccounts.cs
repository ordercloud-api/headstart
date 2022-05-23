using System.Collections.Generic;
using System.Linq;

namespace OrderCloud.Integrations.EasyPost.Models
{
    public class HSShippingProfiles : EasyPostShippingProfiles
    {
        public HSShippingProfiles(string carrierAccountId)
        {
            // TODO: Replace this with shipping profiles needed
            this.ShippingProfiles.Add(new EasyPostShippingProfile()
            {
                ID = "SMG",
                SupplierID = null,
                CarrierAccountIDs = new List<string>() { carrierAccountId },
                Customs_Signer = "Bob Bernier",
                Restriction_Type = "none",
                EEL_PFC = "NOEEI30.37(a)",
                Customs_Certify = true,
                Markup = 1.5M,
                Default = true,
            });
        }

        public override EasyPostShippingProfile FirstOrDefault(string id)
        {
            return ShippingProfiles.FirstOrDefault(p => p.SupplierID == id) ?? ShippingProfiles.First(p => p.ID == "SMG");
        }
    }
}
