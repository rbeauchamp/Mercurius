using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Mercurius
{
    public class MessageDispatcher
    {
        private readonly IEnumerable<IMessageHandler> _messageHandlers;
        private readonly ILogger<MessageDispatcher> _logger;

        public MessageDispatcher(IEnumerable<IMessageHandler> messageHandlers, ILogger<MessageDispatcher> logger)
        {
            _messageHandlers = messageHandlers;
            _logger = logger;
        }

        //public IEnumerable<IMessageHandler> MessageHandlers { get; }

        public async Task DispatchAsync(IMessage message)
        {
            var tasks = _messageHandlers
                .Where(handler => handler.MessageTypes.Any(type => type.IsInstanceOfType(message)))
                .Select(handler => HandleAsync(message, handler));

            await Task.WhenAll(tasks);
        }

        public async Task<bool> TryDispatchToSingleHandlerAsync(IMessage message)
        {
            try
            {
                return await _messageHandlers
                    .Single(handler => handler.MessageTypes.Any(type => type.IsInstanceOfType(message)))
                    .TryHandleAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return false;
        }

        private static async Task HandleAsync(IMessage message, IMessageHandler handler)
        {
            await handler.HandleAsync(message);
        }
    }
}