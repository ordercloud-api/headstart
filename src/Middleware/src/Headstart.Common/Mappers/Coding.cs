using Headstart.Common.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace Headstart.Common.Mappers
{
	public static class Coding
	{
		// https://api.anytimefitness.com/Help/SSO#login-styling
		public static string EncodeState(SSOState state)
		{
			var json = JsonConvert.SerializeObject(state);
			return Base64UrlEncoder.Encode(json);
		}

		public static SSOState DecodeState(string state)
		{
			var json = Base64UrlEncoder.Decode(state);
			return JsonConvert.DeserializeObject<SSOState>(json);
		}

		public static string GenerateCodeChallange(string codeVerifier)
		{
			using (SHA256 sha256Hash = SHA256.Create())
			{
				//From String to byte array
				byte[] sourceBytes = Encoding.UTF8.GetBytes(codeVerifier);
				byte[] hashBytes = sha256Hash.ComputeHash(sourceBytes);
				return Base64UrlEncoder.Encode(hashBytes);
			}
		}
	}
}
