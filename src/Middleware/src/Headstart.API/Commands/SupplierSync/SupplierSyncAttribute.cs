using System;

namespace Headstart.API.Commands
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class SupplierSyncAttribute : Attribute
	{
		/// <summary>
		/// The Default constructor method for the SupplierSyncAttribute class object
		/// </summary>
		/// <param name="supplierID"></param>
		public SupplierSyncAttribute(string supplierID)
		{
			this.SupplierID = supplierID;
		}
		
		public string SupplierID { get; set; }
	}
}
