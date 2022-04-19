using System;

namespace Headstart.API.Commands.SupplierSync
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class SupplierSyncAttribute : Attribute
	{
		public string SupplierId { get; set; }

		/// <summary>
		/// The Default constructor method for the SupplierSyncAttribute class object
		/// </summary>
		/// <param name="supplierId"></param>
		public SupplierSyncAttribute(string supplierId)
		{
			SupplierId = supplierId;
		}
	}
}