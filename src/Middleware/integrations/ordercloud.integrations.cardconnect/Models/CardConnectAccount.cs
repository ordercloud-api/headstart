
namespace ordercloud.integrations.cardconnect
{
    public class CardConnectAccountRequest
    {
        public string account { get; set; }
        public string currency { get; set; }
    }


    public class CardConnectAccountResponse
    {
        public string message { get; set; }
        public int errorcode { get; set; }
        public string token { get; set; }
    }

}
