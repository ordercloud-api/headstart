using System.Collections.Generic;
using System.Threading.Tasks;
using Headstart.Common.Models;

namespace Headstart.Common.Services
{
    public class DefaultOMSService : IOMSService
    {
        public Task<ProcessResult> ExportOrder(HSOrderWorksheet worksheet, IList<HSOrder> supplierOrders, bool isOrderSubmit = true)
        {
            return Task.FromResult<ProcessResult>(null);
        }
    }
}
