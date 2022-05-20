using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using OrderCloud.Integrations.Smarty.Models;
using SmartyStreets.USStreetApi;

namespace OrderCloud.Integrations.Smarty
{
    public interface ISmartyStreetsService
    {
        // returns many incomplete address suggestions
        Task<AutoCompleteResponse> USAutoCompletePro(string search);

        // returns one or zero very complete suggestions
        Task<List<Candidate>> ValidateSingleUSAddress(Lookup lookup);
    }

    public class SmartyStreetsService : ISmartyStreetsService
    {
        private readonly SmartyStreetsConfig config;
        private readonly Client smartyStreetsClients;
        private readonly string autoCompleteBaseUrl = "https://us-autocomplete-pro.api.smartystreets.com";

        public SmartyStreetsService(SmartyStreetsConfig config, Client smartyStreetsClients)
        {
            this.config = config;
            this.smartyStreetsClients = smartyStreetsClients;
        }

        public async Task<List<Candidate>> ValidateSingleUSAddress(Lookup lookup)
        {
            smartyStreetsClients.Send(lookup);
            return await Task.FromResult(lookup.Result);
        }

        public async Task<AutoCompleteResponse> USAutoCompletePro(string search)
        {
            var suggestions = await autoCompleteBaseUrl
                .AppendPathSegment("lookup")
                .SetQueryParam("key", config.WebsiteKey)
                .SetQueryParam("search", search)
                .WithHeader("Referer", config.RefererHost)
                .GetJsonAsync<AutoCompleteResponse>();

            return suggestions;
        }
    }
}
