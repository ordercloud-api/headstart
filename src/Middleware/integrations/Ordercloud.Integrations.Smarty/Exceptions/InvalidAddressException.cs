using OrderCloud.Catalyst;
using OrderCloud.Integrations.Smarty.Models;

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
