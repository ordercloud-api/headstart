﻿using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Headstart.Models.Headstart
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum VerifiedUserType
	{
		supplier,
		admin,
		buyer,

		// not a user type being returned from token, used to represent states without verified user
		noUser,
	}
}
