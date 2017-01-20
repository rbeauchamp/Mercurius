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

        public async Task DispatchAsync(IMessage message, Principal principal)
        {
            var tasks = _messageHandlers
                .Where(handler => handler.MessageTypes.Any(type => type.IsInstanceOfType(message)))
                .Select(async handler => await handler.HandleAsync(message, principal));

            await Task.WhenAll(tasks);
        }

        public async Task<bool> TryDispatchToSingleHandlerAsync(IMessage message, Principal principal)
        {
            try
            {
                return await _messageHandlers
                    .Single(handler => handler.MessageTypes.Any(type => type.IsInstanceOfType(message)))
                    .TryHandleAsync(message, principal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return false;
        }
    }
}