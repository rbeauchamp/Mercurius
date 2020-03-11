using System;
using System.Collections.Generic;
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
        /// The types of messages handled by this message handler.
        /// </summary>
        /// <remarks>
        /// The types must implement <see cref="IMessage"/>.
        /// </remarks>
        IEnumerable<Type> MessageTypes { get; }

        /// <summary>
        /// Get the results of the query.
        /// </summary>
        Task<T> TryGetAsync<T>(IQuery<T> query, IPrincipal principal);

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
    }
}