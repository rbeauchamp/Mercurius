using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Mercurius
{
    /// <inheritdoc />
    public abstract class MessageHandler : IMessageHandler
    {
        public abstract IEnumerable<Type> MessageTypes { get; }

        public virtual Task<bool> TryHandleAsync(Event @event, IPrincipal principal)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            throw new NotImplementedException($"You must override this method to handle the message type {@event.GetType()}");
        }

        public virtual Task<bool> TryHandleAsync(Command command, IPrincipal principal)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            throw new NotImplementedException($"You must override this method to handle the message type {command.GetType()}");
        }

        public virtual Task<T> TryGetAsync<T>(IQuery<T> query, IPrincipal principal)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            throw new NotImplementedException($"You must override this method to handle the query type {query.GetType()}");
        }
    }
}