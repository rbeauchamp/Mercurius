using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mercurius
{
    public class MessageDispatcher
    {
        public MessageDispatcher(IServiceProvider serviceProvider, IEnumerable<IMessageHandler> messageHandlers)
        {
            ServiceProvider = serviceProvider;
            MessageHandlers = messageHandlers;
        }

        public IEnumerable<IMessageHandler> MessageHandlers { get; }

        public IServiceProvider ServiceProvider { get; }

        public async Task DispatchAsync(IMessage message)
        {
            var tasks = MessageHandlers
                .Where(handler => handler.MessageTypes.Any(type => type.IsInstanceOfType(message)))
                .Select(handler => HandleAsync(message, handler));

            await Task.WhenAll(tasks);
        }

        private async Task HandleAsync(IMessage message, IMessageHandler handler)
        {
            await handler.HandleAsync(message);
        }

        public async Task DispatchAsync(IEnumerable<IMessage> messages)
        {
            foreach (var message in messages)
            {
                await DispatchAsync(message);
            }
        }
    }
}