using Microsoft.IdentityModel.Tokens;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Headstart.Tests
{
    public static class TestHelpers
    {
        /// <summary>
        /// use to access values on dynamics
        /// </summary>
        /// <param name="source">the thing to drill into</param>
        /// <param name="accessor">typically the name of the property, use dot notation to access nested values</param>
        /// <returns></returns>
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
