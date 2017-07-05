using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mercurius
{
    public class MessageDispatcher : IMessageDispatcher
    {
        private readonly IEnumerable<IMessageHandler> _messageHandlers;

        public MessageDispatcher(IEnumerable<IMessageHandler> messageHandlers)
        {
            _messageHandlers = messageHandlers;
        }

        public async Task DispatchAsync(Event @event, Principal principal)
        {
            var messageHandlers = _messageHandlers
                .Where(handler => handler.MessageTypes.Any(type => type.IsInstanceOfType(@event)));

            var messageHandlersList = messageHandlers as IList<IMessageHandler> ?? messageHandlers.ToList();

            if (!messageHandlersList.Any())
            {
                throw new Exception($"Could not find at least one handler for the event type {@event.GetType()}");
            }

            var tasks = messageHandlersList.Select(async handler => await handler.HandleAsync(@event, principal).ConfigureAwait(false));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public async Task<bool> TryDispatchAsync(Command command, Principal principal)
        {
            var messageHandler = _messageHandlers
                .SingleOrDefault(handler => handler.MessageTypes.Any(type => type.IsInstanceOfType(command)));

            if (messageHandler == null)
            {
                throw new Exception($"Could not find a handler for the command type {command.GetType()}");
            }

            return await messageHandler.TryHandleAsync(command, principal).ConfigureAwait(false);
        }

        public async Task<IQueryable<T>> DispatchAsync<T>(IQuery<T> query, Principal principal)
        {
            var messageHandler = _messageHandlers
                .SingleOrDefault(handler => handler.MessageTypes.Any(type => type.IsInstanceOfType(query)));

            if (messageHandler == null)
            {
                throw new Exception($"Could not find a handler for the query type {query.GetType()}");
            }

            return await messageHandler.GetAsync(query, principal).ConfigureAwait(false);
        }
    }
}