using OrderCloud.Catalyst;

namespace ordercloud.integrations.smartystreets
{
    public class InvalidAddressException : CatalystBaseException
    {
        public InvalidAddressException(AddressValidation validation)
            : base("InvalidAddress", "Address not valid", validation, 400)
        {
        }
    }
}
