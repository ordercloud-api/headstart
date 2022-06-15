using System.Threading.Tasks;
using Flurl.Http;
using OrderCloud.Integrations.EasyPost.Exceptions;
using OrderCloud.Integrations.EasyPost.Models;

namespace OrderCloud.Integrations.EasyPost
{
    public class EasyPostClient
    {
        private const string BaseUrl = "https://api.easypost.com/v2";
        private readonly string apiKey;

        public EasyPostClient(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public async Task<EasyPostShipment> PostShipment(EasyPostShipment shipment)
        {
            try
            {
                return await BaseUrl
                    .WithBasicAuth(apiKey, string.Empty)
                    .AppendPathSegment("shipments")
                    .PostJsonAsync(new { shipment })
                    .ReceiveJson<EasyPostShipment>();
            }
            catch (FlurlHttpException ex)
            {
                var error = await ex.GetResponseJsonAsync<EasyPostApiError>();
                throw new EasyPostException(error);
            }
        }
    }
}
