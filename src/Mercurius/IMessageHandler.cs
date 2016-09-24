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
        /// Handles the mesage.
        /// </summary>
        Task HandleAsync(Message message);

        /// <summary>
        /// The types of messages handled by this message handler.
        /// </summary>
        /// <remarks>
        /// The types must be a sub-class of <see cref="Message"/>.
        /// </remarks>
        IEnumerable<Type> MessageTypes { get; }
    }
}