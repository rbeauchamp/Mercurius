using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mercurius
{
    public abstract class MessageHandler : IMessageHandler
    {
        protected MessageHandler()
        {
        }

        public async Task HandleAsync(Message message)
        {
            // Dispatch message to the subtype
            await OnHandleAsync(message);
        }

        protected abstract Task OnHandleAsync(Message message);

        /// <summary>
        /// The types of messages handled by this message handler.
        /// </summary>
        /// <remarks>
        /// The types must be a sub-class of <see cref="Message"/>.
        /// </remarks>
        public abstract IEnumerable<Type> MessageTypes { get; }
    }
}