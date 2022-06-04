using System.Threading.Tasks;
using Headstart.Common.Models;

namespace Headstart.Common.Services
{
    public interface ICurrencyConversionService
    {
        Task<ConversionRates> Get(CurrencyCode currencyCode);
    }
}
