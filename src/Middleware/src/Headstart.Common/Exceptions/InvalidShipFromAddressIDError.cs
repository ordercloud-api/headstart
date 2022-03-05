namespace Headstart.Models.Exceptions
{
	public class InvalidShipFromAddressIDError
	{
		public string ShipFromAddressID { get; set; } = string.Empty;

		public InvalidShipFromAddressIDError(string id)
		{
			ShipFromAddressID = id;
		}
	}
}