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
            throw new NotImplementedException($"You must override this method to handle the message type {@event.GetType()}");
        }

        public virtual Task<bool> TryHandleAsync(Command command, IPrincipal principal)
        {
            throw new NotImplementedException($"You must override this method to handle the message type {command.GetType()}");
        }

        public virtual Task<T> TryGetAsync<T>(IQuery<T> query, IPrincipal principal)
        {
            throw new NotImplementedException($"You must override this method to handle the query type {query.GetType()}");
        }
    }
}