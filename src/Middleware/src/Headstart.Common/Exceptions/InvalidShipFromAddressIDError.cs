namespace Headstart.Models.Exceptions
{
	public class InvalidShipFromAddressIDError
	{
		public InvalidShipFromAddressIDError(string id)
		{
			ShipFromAddressID = id;
		}

		public string ShipFromAddressID { get; set; } 
	}
}
