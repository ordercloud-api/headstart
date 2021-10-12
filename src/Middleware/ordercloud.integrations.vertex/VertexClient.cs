using Flurl.Http;
using Flurl;
using ordercloud.integrations.vertex.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ordercloud.integrations.vertex
{
	public class VertexClient
	{
		private const string ApiUrl = "https://restconnect.vertexsmb.com";
		private const string AuthUrl = "https://auth.vertexsmb.com";
		private readonly VertexConfig _config;
		private VertexTokenResponse _token;

		public VertexClient(VertexConfig config)
		{
			_config = config;
		}

		public async Task<VertexCalculateTaxResponse> CalculateTax(VertexCalculateTaxRequest request)
		{
			if (_token?.access_token == null)
			{
				_token = await GetToken(_config);
			}

			var response = await $"{ApiUrl}/vertex-restapi/v1/sale"
				.WithOAuthBearerToken(_token.access_token)
				.PostJsonAsync(request);

			var taxCalculation = await response.GetJsonAsync<VertexCalculateTaxResponse>();
			return taxCalculation;
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
				password = config.Password
			};
			var response = await $"{AuthUrl}/identity/connect/token".PostUrlEncodedAsync(body);
			var token = await response.GetJsonAsync<VertexTokenResponse>();
			return token;
		}
	}
}
