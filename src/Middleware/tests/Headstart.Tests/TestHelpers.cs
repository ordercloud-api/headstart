using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Headstart.Tests
{
    public static class TestHelpers
    {
        /// <summary>
        /// use to access values on dynamics.
        /// </summary>
        /// <param name="source">the thing to drill into.</param>
        /// <param name="accessor">typically the name of the property, use dot notation to access nested values.</param>
        public static dynamic GetDynamicVal(dynamic source, string accessor)
        {
            var props = accessor.Split('.');
            foreach (var prop in props)
            {
                source = source.GetType().GetProperty(prop).GetValue(source, null);
            }

            return source;
        }

        public static string MockOrderCloudToken(string clientID = "mockClientID")
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("blahblahblahblahblahblahblahblahblahblah"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "mydomain.com",
                audience: "mydomain.com",
                claims: new[] { new Claim("cid", clientID) },
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
