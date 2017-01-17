using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mercurius
{
    public abstract class MessageHandler : IMessageHandler
    {
        public async Task HandleAsync(IMessage message)
        {
            // Dispatch message to the subtype
            await OnHandleAsync(message);
        }

        protected abstract Task OnHandleAsync(IMessage message);

        /// <summary>
        /// The types of messages handled by this message handler.
        /// </summary>
        /// <remarks>
        /// The types must be a sub-class of <see cref="Message"/>.
        /// </remarks>
        public abstract IEnumerable<Type> MessageTypes { get; }

        public virtual Task<bool> TryHandleAsync(IMessage message)
        {
            throw new NotImplementedException($"You must override this method to handle the message type {message.GetType()}");
        }
    }
}