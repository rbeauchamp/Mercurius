using System.Linq;
using System.Threading.Tasks;

namespace Mercurius
{
    public interface IMessageDispatcher
    {
        /// <summary>
        /// Dispatch the event to all <see cref="MessageHandler"/>s configured to handle it.
        /// </summary>
        /// <param name="event">The event.</param>
        /// <param name="principal">The principal dispatching the event.</param>
        Task DispatchAsync(Event @event, Principal principal);

        /// <summary>
        /// Dispatch the query to the <see cref="MessageHandler" /> configured to handle it.
        /// </summary>
        /// <typeparam name="T">The type returned by the query.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="principal">The principal dispatching the query.</param>
        /// <returns>A non-null queryable of the given <typeparamref name="T"/></returns>
        Task<IQueryable<T>> DispatchAsync<T>(IQuery<T> query, Principal principal);

        /// <summary>
        /// Dispatch the command to the <see cref="MessageHandler"/> configured to handle it.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="principal">The principal dispatching the command.</param>
        Task<bool> TryDispatchAsync(Command command, Principal principal);
    }
}