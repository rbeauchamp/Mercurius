using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mercurius
{
    public interface IMessageHandler
    {
        Task HandleAsync(Message message);

        IEnumerable<Type> MessageTypes { get; }
    }
}