using OrderCloud.Catalyst;

namespace ordercloud.integrations.smartystreets
{
    public class InvalidBuyerAddressException : CatalystBaseException
    {
        public InvalidBuyerAddressException(BuyerAddressValidation validation)
            : base("InvalidAddress", "Address not valid", validation, 400)
        {
        }
    }
}
