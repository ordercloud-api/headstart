using System;

namespace Headstart.Orchestration
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SupplierSyncAttribute : Attribute
    {
        public string SupplierID { get; set; }

        public SupplierSyncAttribute(string supplierID)
        {
            this.SupplierID = supplierID;
        }
    }
}
