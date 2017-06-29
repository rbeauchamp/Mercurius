using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mercurius
{
    public abstract class MessageHandler : IMessageHandler
    {
        public abstract IEnumerable<Type> MessageTypes { get; }

        public virtual Task HandleAsync(Event @event, Principal principal)
        {
            throw new NotImplementedException($"You must override this method to handle the message type {@event.GetType()}");
        }

        public virtual Task<bool> TryHandleAsync(Command command, Principal principal)
        {
            throw new NotImplementedException($"You must override this method to handle the message type {command.GetType()}");
        }

        public virtual Task<IQueryable<T>> HandleAsync<T>(IQuery<T> query, Principal principal) where T : class
        {
            throw new NotImplementedException($"You must override this method to handle the query type {query.GetType()}");
        }
    }
}