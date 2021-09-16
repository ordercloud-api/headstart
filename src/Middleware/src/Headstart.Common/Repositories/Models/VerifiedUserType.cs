using Newtonsoft.Json.Converters;
using OrderCloud.SDK;
using System.Text.Json.Serialization;

namespace Headstart.Models.Headstart
{
    // Todo - can this be totally replaced by CommerceRole?
    [JsonConverter(typeof(StringEnumConverter))]
    public enum VerifiedUserType
    {
        supplier, admin, buyer, 
        
        // not a user type being returned from token, used to represent states without verified user
        noUser
    }


    public static class CommerceRoleExtensions
	{
        public static VerifiedUserType ToVerifiedUserType(this CommerceRole role)
		{
            switch (role)
			{
                case CommerceRole.Buyer:
                    return VerifiedUserType.buyer;
                case CommerceRole.Supplier:
                    return VerifiedUserType.supplier;
                case CommerceRole.Seller:
					return VerifiedUserType.admin;
                default:
                    return VerifiedUserType.noUser;
			}
		}
    }
}
