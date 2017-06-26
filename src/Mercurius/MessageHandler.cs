using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mercurius
{
    public abstract class MessageHandler : IMessageHandler
    {
        public abstract IEnumerable<Type> MessageTypes { get; }

        public async Task HandleAsync(IMessage message, Principal principal)
        {
            // Dispatch message to the subtype
            await OnHandleAsync(message, principal).ConfigureAwait(false);
        }
        protected abstract Task OnHandleAsync(IMessage message, Principal principal);

        public virtual Task<bool> TryHandleAsync(IMessage message, Principal principal)
        {
            throw new NotImplementedException($"You must override this method to handle the message type {message.GetType()}");
        }
    }
}