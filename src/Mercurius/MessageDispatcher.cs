using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Mercurius.Validations;

namespace Mercurius
{
    /// <inheritdoc />
    public class MessageDispatcher : IMessageDispatcher
    {
        private readonly IEnumerable<IMessageHandler> _messageHandlers;
        private readonly IServiceProvider _serviceProvider;

        public MessageDispatcher(IEnumerable<IMessageHandler> messageHandlers, IServiceProvider serviceProvider)
        {
            _messageHandlers = messageHandlers;
            _serviceProvider = serviceProvider;
        }

        public async Task<bool> TryDispatchAsync(Command command, IPrincipal principal)
        {
            await command.IsValidOrThrowExceptionAsync(_serviceProvider, principal).ConfigureAwait(false);

            var messageHandler = _messageHandlers
                .SingleOrDefault(handler => handler.MessageTypes.Any(type => type.IsInstanceOfType(command)));

            if (messageHandler == null)
            {
                return true;
            }

            return await messageHandler.TryHandleAsync(command, principal).ConfigureAwait(false);
        }

        public async Task<bool> TryDispatchAsync(Event @event, IPrincipal principal)
        {
            await @event.IsValidOrThrowExceptionAsync(_serviceProvider, principal).ConfigureAwait(false);

            var messageHandlers = _messageHandlers
                .Where(handler => handler.MessageTypes.Any(type => type.IsInstanceOfType(@event)));

            var messageHandlersList = messageHandlers as IList<IMessageHandler> ?? messageHandlers.ToList();

            if (!messageHandlersList.Any())
            {
                return true;
            }

            var tasks = messageHandlersList.Select(async handler => await handler.TryHandleAsync(@event, principal).ConfigureAwait(false));

            return (await Task.WhenAll(tasks).ConfigureAwait(false)).All(result => result);
        }


        public async Task<T> TryDispatchAsync<T>(IQuery<T> query, IPrincipal principal)
        {
            await query.IsValidOrThrowExceptionAsync(_serviceProvider, principal).ConfigureAwait(false);

            var messageHandler = _messageHandlers
                .SingleOrDefault(handler => handler.MessageTypes.Any(type => type.IsInstanceOfType(query)));

            if (messageHandler == null)
            {
                return default;
            }

            return await messageHandler.TryGetAsync(query, principal).ConfigureAwait(false);
        }
    }
}