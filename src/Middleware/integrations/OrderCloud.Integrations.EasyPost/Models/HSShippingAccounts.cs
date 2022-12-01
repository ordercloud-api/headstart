using System.Collections.Generic;
using System.Linq;

namespace OrderCloud.Integrations.EasyPost.Models
{
    public class HSShippingProfiles : EasyPostShippingProfiles
    {
        public HSShippingProfiles(string customsSigner, string fedexAccountId, string upsAccountId)
        {
            if (!string.IsNullOrEmpty(fedexAccountId))
            {
                this.ShippingProfiles.Add(new EasyPostShippingProfile()
                {
                    ID = "FEDEX_CARRIER",
                    SupplierID = null,
                    CarrierAccountIDs = new List<string>() { fedexAccountId },
                    Customs_Signer = customsSigner,
                    Restriction_Type = "none",
                    EEL_PFC = "NOEEI30.37(a)",
                    Customs_Certify = true,
                    Markup = 1.5M,
                    Default = true,
                });
            }

            if (!string.IsNullOrEmpty(upsAccountId))
            {
                this.ShippingProfiles.Add(new EasyPostShippingProfile()
                {
                    ID = "UPS_CARRIER",
                    SupplierID = null,
                    CarrierAccountIDs = new List<string>() { upsAccountId },
                    Customs_Signer = customsSigner,
                    Restriction_Type = "none",
                    EEL_PFC = "NOEEI30.37(a)",
                    Customs_Certify = true,
                    Markup = 1.5M,
                    Default = true,
                });
            }
        }

        public override EasyPostShippingProfile FirstOrDefault(string id)
        {
            return ShippingProfiles.FirstOrDefault(p => p.SupplierID == id) ?? ShippingProfiles.First(p => p.ID == "SMG");
        }
    }
}
