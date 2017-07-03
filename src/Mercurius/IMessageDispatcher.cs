using System.Linq;
using System.Threading.Tasks;

namespace Mercurius
{
    public interface IMessageDispatcher
    {
        Task DispatchToAllAsync(Event @event, Principal principal);

        Task<IQueryable<T>> DispatchToSingleAsync<T>(IQuery<T> query, Principal principal);

        Task<bool> TryDispatchToSingleAsync(Command command, Principal principal);
    }
}