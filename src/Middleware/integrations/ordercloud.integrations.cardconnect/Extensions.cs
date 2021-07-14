using System.Runtime.CompilerServices;
using OrderCloud.SDK;

namespace ordercloud.integrations.cardconnect
{
    public static class Extensions
    {
        public static string ToCreditCardDisplay(this string value)
        {
            var result = $"{value.Substring(value.Length - 4, 4)}";
            return result;
        }

        public static bool IsValidCvv(this OrderCloudIntegrationsCreditCardPayment payment, BuyerCreditCard cc)
        {
            // if credit card is direct without using a saved card then consider it a ME card and should enforce CVV
            // saved credit cards for ME just require CVV
            return (payment.CreditCardDetails == null || payment.CVV != null) && (!cc.Editable || payment.CVV != null);
        }

        public static bool WasSuccessful(this CardConnectVoidResponse attempt)
        {
            // If the void is successful, the authcode will contain REVERS. If transaction is not found or an error occurs:
            // Identifies if the void was successful.Can one of the following values: 
            // REVERS - Successful
            // Null - Unsuccessful.Refer to the respcode and resptext.
            return attempt.authcode == "REVERS";
        }

        public static bool WasSuccessful(this CardConnectInquireResponse attempt)
        {
            return attempt.respstat == "A";
        }

        public static bool WasSuccessful(this CardConnectAuthorizationResponse attempt)
        {
            return attempt.respstat == "A" && (attempt.respcode == "0" || attempt.respcode == "00" || attempt.respcode == "000");
        }
        public static bool IsExpired(this CardConnectAuthorizationResponse attempt)
        {
            return attempt.respcode == "101";
        }

        public static bool IsDeclined(this CardConnectAuthorizationResponse attempt)
        {
            return attempt.respcode == "500";
        }

        public static bool PassedAVSCheck(this CardConnectAuthorizationResponse attempt)
        {
            if (attempt.WasSuccessful()) return true;
            return (attempt.avsresp != null && (attempt.avsresp != "N" && attempt.avsresp != "A" && attempt.avsresp != "Z"));
        }

        public static bool PassedCvvCheck(this CardConnectAuthorizationResponse attempt, CardConnectAuthorizationRequest request)
        {
            if (attempt.WasSuccessful()) return true;
            if (request.cvv2 == null && (attempt.cvvresp == "P" || attempt.cvvresp == null)) return true;
            return (attempt.cvvresp != null && attempt.cvvresp != "N" && attempt.cvvresp != "P" && attempt.cvvresp != "U");
        }

        public static bool WasSuccessful(this CardConnectRefundResponse attempt)
        {
            return attempt.respstat == "A";
        }
    }
}
