namespace Headstart.Common.Exceptions
{
	public class InvalidShipFromAddressIdError
	{
		public string ShipFromAddressId { get; set; } = string.Empty;

		public InvalidShipFromAddressIdError(string id)
		{
			ShipFromAddressId = id;
		}
	}
}