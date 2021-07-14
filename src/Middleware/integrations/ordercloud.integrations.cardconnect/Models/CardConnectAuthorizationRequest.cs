using ordercloud.integrations.library;

namespace ordercloud.integrations.cardconnect
{
   public class CardConnectAuthorizationRequest
    {
        public string merchid { get; set; }
        public string orderid { get; set; }
        public string account { get; set; }
        public string expiry { get; set; }
        public string amount { get; set; }
        public string currency { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string region { get; set; }
        public string country { get; set; }
        public string postal { get; set; }
        ////public string profile { get; set; }
        public string ecomind { get; set; } = "E";
        public string cvv2 { get; set; }
        public string capture { get; set; }
        ////public object track { get; set; }
        //public string capture { get; set; }
        //public string bin { get; set; }
    }

    public class CardConnectAuthorizationResponse
    {
        public string token { get; set; }
        public string account { get; set; }
        public string retref { get; set; }
        public decimal? amount { get; set; }
        public string expiry { get; set; }
        public string merchid { get; set; }
        public string avsresp { get; set; }
        public string cvvresp { get; set; }
        public string signature { get; set; }
        public string bintype { get; set; }
        public string commcard { get; set; }
        public string emv { get; set; }
        public BinInfo binInfo { get; set; }
        public dynamic receipt { get; set; }
        public string authcode { get; set; }
        public string respcode { get; set; }
        public string respproc { get; set; }
        public string respstat { get; set; }
        public string resptext { get; set; }
    }

    public class BinInfo
    {
        public string country { get; set; }
        public string product { get; set; }
        public string bin { get; set; }
        public string cardusestring { get; set; }
        public bool gsa { get; set; }
        public bool corporate { get; set; }
        public bool fsa { get; set; }
        public string subtype { get; set; }
        public bool purchase { get; set; }
        public bool prepaid { get; set; }
        public string issuer { get; set; }
        public string binlo { get; set; }
        public string binhi { get; set; }
    }

}
