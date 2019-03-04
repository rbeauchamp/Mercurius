using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Mercurius
{
    /// <summary>
    /// Implements domain logic upon handling a message.
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// Handle the event.
        /// </summary>
        [Obsolete("HandleAsync is deprecated, please use TryHandleAsync instead.")]
        Task HandleAsync(Event @event, IPrincipal principal);

        /// <summary>
        /// Get the results of query.
        /// </summary>
        Task<IQueryable<T>> GetAsync<T>(IQuery<T> query, IPrincipal principal);

        /// <summary>
        /// Try to handle the command.
        /// If successfully handled, return true,
        /// otherwise, false.
        /// </summary>
        Task<bool> TryHandleAsync(Command command, IPrincipal principal);

        /// <summary>
        /// Try to handle the event.
        /// If successfully handled, return true,
        /// otherwise, false.
        /// </summary>
        Task<bool> TryHandleAsync(Event @event, IPrincipal principal);

        /// <summary>
        /// The types of messages handled by this message handler.
        /// </summary>
        /// <remarks>
        /// The types must implement <see cref="IMessage"/>.
        /// </remarks>
        IEnumerable<Type> MessageTypes { get; }
    }
}