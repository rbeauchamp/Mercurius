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

        public abstract IEnumerable<Type> MessageTypes { get; }
    }
}