using Headstart.Common.Models;
using OrderCloud.Catalyst;

namespace OrderCloud.Integrations.Smarty.Exceptions
{
    public class InvalidBuyerAddressException : CatalystBaseException
    {
        public InvalidBuyerAddressException(BuyerAddressValidation validation)
            : base("InvalidAddress", "Address not valid", validation, 400)
        {
        }
    }
}
