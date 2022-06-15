using Headstart.Common.Models;
using OrderCloud.Catalyst;

namespace OrderCloud.Integrations.Smarty.Exceptions
{
    public class InvalidAddressException : CatalystBaseException
    {
        public InvalidAddressException(AddressValidation validation)
            : base("InvalidAddress", "Address not valid", validation, 400)
        {
        }
    }
}
