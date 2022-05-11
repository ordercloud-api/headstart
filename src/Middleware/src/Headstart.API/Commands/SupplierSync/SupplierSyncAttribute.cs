using System;

namespace Headstart.API.Commands.SupplierSync
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class SupplierSyncAttribute : Attribute
	{
		public string SupplierID { get; set; }

		/// <summary>
		/// The Default constructor method for the SupplierSyncAttribute class object
		/// </summary>
		/// <param name="supplierID"></param>
		public SupplierSyncAttribute(string supplierID)
		{
			SupplierID = supplierID;
		}
	}
}