using Flurl.Http;
using ordercloud.integrations.vertex.Models;
using System;
using System.Threading.Tasks;
using OrderCloud.Catalyst;

namespace ordercloud.integrations.vertex
{
    public class VertexClient
    {
        private const string ApiUrl = "https://restconnect.vertexsmb.com";
        private const string AuthUrl = "https://auth.vertexsmb.com";
        private readonly VertexConfig config;
        private VertexTokenResponse token;

        public VertexClient(VertexConfig config)
        {
            this.config = config;
        }

        public async Task<VertexCalculateTaxResponse> CalculateTax(VertexCalculateTaxRequest request)
        {
            return await MakeRequest<VertexCalculateTaxResponse>(() =>
                $"{ApiUrl}/vertex-restapi/v1/sale"
                    .WithOAuthBearerToken(token.access_token)
                    .AllowAnyHttpStatus()
                    .PostJsonAsync(request));
        }

        private async Task<T> MakeRequest<T>(Func<Task<IFlurlResponse>> request)
        {
            if (token?.access_token == null)
            {
                token = await GetToken(config);
            }

            var response = await (await request()).GetJsonAsync<VertexResponse<T>>();
            if (response.errors.Exists(e => e.detail == "invalid access token"))
            {
                // refresh the token
                token = await GetToken(config);

                // try the request again
                response = await (await request()).GetJsonAsync<VertexResponse<T>>();
            }

            // Catch and throw any api errors
            Require.That(response.errors.Count == 0, new VertexException(response.errors));

            return response.data;
        }

        private async Task<VertexTokenResponse> GetToken(VertexConfig config)
        {
            var body = new
            {
                scope = "calc-rest-api",
                grant_type = "password",
                client_id = config.ClientID,
                client_secret = config.ClientSecret,
                username = config.Username,
                password = config.Password,
            };
            var response = await $"{AuthUrl}/identity/connect/token".PostUrlEncodedAsync(body);
            var token = await response.GetJsonAsync<VertexTokenResponse>();
            return token;
        }
    }
}
