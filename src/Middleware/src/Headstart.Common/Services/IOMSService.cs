using System.Collections.Generic;
using System.Threading.Tasks;
using Headstart.Common.Models;

namespace Headstart.Common.Services
{
    public interface IOMSService
    {
        Task<ProcessResult> ExportOrder(HSOrderWorksheet worksheet, IList<HSOrder> supplierOrders, bool isOrderSubmit = true);
    }
}
