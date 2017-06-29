using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mercurius
{
    public sealed class MessageDispatcher
    {
        private readonly IEnumerable<IMessageHandler> _messageHandlers;

        public MessageDispatcher(IEnumerable<IMessageHandler> messageHandlers)
        {
            _messageHandlers = messageHandlers;
        }

        public async Task DispatchToAllAsync(Event @event, Principal principal)
        {
            var tasks = _messageHandlers
                .Where(handler => handler.MessageTypes.Any(type => type.IsInstanceOfType(@event)))
                .Select(async handler => await handler.HandleAsync(@event, principal).ConfigureAwait(false));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public async Task<bool> TryDispatchToSingleAsync(Command command, Principal principal)
        {
            return await _messageHandlers
                .Single(handler => handler.MessageTypes.Any(type => type.IsInstanceOfType(command)))
                .TryHandleAsync(command, principal).ConfigureAwait(false);
        }

        public async Task<IQueryable<T>> DispatchToSingleAsync<T>(IQuery<T> query, Principal principal)
            where T : class
        {
            return await _messageHandlers
                .Single(handler => handler.MessageTypes.Any(type => type.IsInstanceOfType(query)))
                .HandleAsync(query, principal).ConfigureAwait(false);
        }
    }
}