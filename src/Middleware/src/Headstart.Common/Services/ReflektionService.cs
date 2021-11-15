using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Headstart.Common.Services
{
	public class ReflektionAccessToken
	{
		public string accessToken { get; set; }
		public int accessTokenExpiry { get; set; }
		public string refreshToken { get; set; }
		public int refreshTokenExpiry { get; set; }
	}

	public interface IReflektionService
	{
		Task<ReflektionAccessToken> GetAccessToken();
	}

	public class ReflektionService : IReflektionService
	{
		private readonly ReflektionSettings _settings;
		public ReflektionService(AppSettings settings)
		{
			_settings = settings.ReflektionSettings;
		}

		public async Task<ReflektionAccessToken> GetAccessToken()
		{
			var response = await _settings.AuthUrl
				.WithHeader("x-api-key", _settings.APIKey)
				.PostJsonAsync(new object { });
			var token = await response.GetJsonAsync<ReflektionAccessToken>();
			return token;		
		}
	}
}
