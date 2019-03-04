using System;
using System.Linq;
using System.Security.Principal;
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
        [Obsolete("DispatchAsync is deprecated, please use TryDispatchAsync instead.")]
        Task DispatchAsync(Event @event, IPrincipal principal);

        /// <summary>
        /// Dispatch the query to the <see cref="MessageHandler" /> configured to handle it.
        /// </summary>
        /// <typeparam name="T">The type returned by the query.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="principal">The principal dispatching the query.</param>
        /// <returns>A non-null queryable of the given <typeparamref name="T"/></returns>
        Task<IQueryable<T>> DispatchAsync<T>(IQuery<T> query, IPrincipal principal);

        /// <summary>
        /// Dispatch the command to the <see cref="MessageHandler"/> configured to handle it.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="principal">The principal dispatching the command.</param>
        Task<bool> TryDispatchAsync(Command command, IPrincipal principal);

        /// <summary>
        /// Dispatch the event to all <see cref="MessageHandler"/>s configured to handle it.
        /// If the dispatch to any instance returns false, then the this method will return false.
        /// Therefore, event handling must be implemented in an idempotent manner.
        /// </summary>
        /// <param name="event">The event.</param>
        /// <param name="principal">The principal dispatching the command.</param>
        Task<bool> TryDispatchAsync(Event @event, IPrincipal principal);
    }
}