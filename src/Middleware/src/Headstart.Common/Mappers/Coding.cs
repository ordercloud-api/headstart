using System.Text;
using Newtonsoft.Json;
using Headstart.Common.Models;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Headstart.Common.Mappers
{
	public static class Coding
	{
		/// <summary>
		/// https://api.anytimefitness.com/Help/SSO#login-styling
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		public static string EncodeState(SsOState state)
		{
			var json = JsonConvert.SerializeObject(state);
			return Base64UrlEncoder.Encode(json);
		}

		public static SsOState DecodeState(string state)
		{
			var json = Base64UrlEncoder.Decode(state);
			return JsonConvert.DeserializeObject<SsOState>(json);
		}

		public static string GenerateCodeChallenge(string codeVerifier)
		{
			using (SHA256 sha256Hash = SHA256.Create())
			{
				//From String to byte array
				var sourceBytes = Encoding.UTF8.GetBytes(codeVerifier);
				var hashBytes = sha256Hash.ComputeHash(sourceBytes);
				return Base64UrlEncoder.Encode(hashBytes);
			}
		}
	}
}