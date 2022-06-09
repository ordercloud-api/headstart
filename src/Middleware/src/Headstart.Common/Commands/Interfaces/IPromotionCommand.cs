using System.Threading.Tasks;

namespace Headstart.Common.Commands
{
    public interface IPromotionCommand
    {
        Task AutoApplyPromotions(string orderID);
    }
}
