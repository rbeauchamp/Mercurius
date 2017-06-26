using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mercurius
{
    public class MessageDispatcher
    {
        private readonly IEnumerable<IMessageHandler> _messageHandlers;

        public MessageDispatcher(IEnumerable<IMessageHandler> messageHandlers)
        {
            _messageHandlers = messageHandlers;
        }

        public async Task DispatchToAllAsync(IMessage message, Principal principal)
        {
            var tasks = _messageHandlers
                .Where(handler => handler.MessageTypes.Any(type => type.IsInstanceOfType(message)))
                .Select(async handler => await handler.HandleAsync(message, principal).ConfigureAwait(false));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public async Task<bool> TryDispatchToSingleAsync(IMessage message, Principal principal)
        {
            return await _messageHandlers
                .Single(handler => handler.MessageTypes.Any(type => type.IsInstanceOfType(message)))
                .TryHandleAsync(message, principal).ConfigureAwait(false);
        }

        public async Task<IQueryable<T>> DispatchToSingleAsync<T>(IQuery<T> query, Principal principal)
        {
            return await _messageHandlers
                .Single(handler => handler.MessageTypes.Any(type => type.IsInstanceOfType(query)))
                .HandleAsync(query, principal).ConfigureAwait(false);
        }
    }
}