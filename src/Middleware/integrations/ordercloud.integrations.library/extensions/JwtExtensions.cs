using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;

namespace ordercloud.integrations.library.extensions
{
	public static class JwtExtensions
	{
		public static string GetClaim(this JwtSecurityToken jwt, string key)
		{
			return jwt.Payload.FirstOrDefault(t => t.Key == key).Value?.ToString();
		}

		public static string GetUserType(this JwtSecurityToken jwt) => jwt.GetClaim("usrtype");

		public static string GetAuthUrl(this JwtSecurityToken jwt) => jwt.GetClaim("iss");

		public static string GetApiUrl(this JwtSecurityToken jwt) => jwt.GetClaim("aud");

		public static string GetClientID(this JwtSecurityToken jwt) => jwt.GetClaim("cid");

		public static DateTime GetExpiresUTC(this JwtSecurityToken jwt) => jwt.GetClaim("exp").UnixToDateTimeUTC();

		public static DateTime GetNotValidBeforeUTC(this JwtSecurityToken jwt) => jwt.GetClaim("nbf").UnixToDateTimeUTC();
	}
}
