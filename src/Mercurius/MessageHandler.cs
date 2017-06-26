using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mercurius
{
    public abstract class MessageHandler : IMessageHandler
    {
        public abstract IEnumerable<Type> MessageTypes { get; }

        public virtual Task HandleAsync(IMessage message, Principal principal)
        {
            throw new NotImplementedException($"You must override this method to handle the message type {message.GetType()}");
        }

        public virtual Task<bool> TryHandleAsync(IMessage message, Principal principal)
        {
            throw new NotImplementedException($"You must override this method to handle the message type {message.GetType()}");
        }

        public virtual Task<IQueryable<T>> HandleAsync<T>(IQuery<T> query, Principal principal)
        {
            throw new NotImplementedException($"You must override this method to handle the query type {query.GetType()}");
        }
    }
}