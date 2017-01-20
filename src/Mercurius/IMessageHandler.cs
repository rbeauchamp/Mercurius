using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mercurius
{
    /// <summary>
    /// Implements domain logic upon handling a message.
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// Handle the message.
        /// </summary>
        Task HandleAsync(IMessage message, Principal principal);

        /// <summary>
        /// Try to handle the message.
        /// If successfully handled, return true,
        /// otherwise, false.
        /// </summary>
        Task<bool> TryHandleAsync(IMessage message, Principal principal);

        /// <summary>
        /// The types of messages handled by this message handler.
        /// </summary>
        /// <remarks>
        /// The types must implement <see cref="IMessage"/>.
        /// </remarks>
        IEnumerable<Type> MessageTypes { get; }
    }
}