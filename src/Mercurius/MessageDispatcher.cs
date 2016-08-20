using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mercurius
{
    public class MessageDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public MessageDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<IMessageHandler> MessageHandlers { get; set; }

        public IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        public async Task DispatchAsync(Message message)
        {
            var tasks = MessageHandlers
                .Where(handler => handler.MessageTypes.Any(type => type.IsInstanceOfType(message)))
                .Select(handler => HandleAsync(message, handler));

            await Task.WhenAll(tasks);
        }

        private async Task HandleAsync(Message message, IMessageHandler handler)
        {
            await handler.HandleAsync(message);
        }

        public async Task DispatchAsync(IEnumerable<Message> messages)
        {
            foreach (var message in messages)
            {
                await DispatchAsync(message);
            }
        }
    }
}