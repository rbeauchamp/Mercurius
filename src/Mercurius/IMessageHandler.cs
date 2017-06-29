﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mercurius
{
    /// <summary>
    /// Implements domain logic upon handling a message.
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// Handle the event.
        /// </summary>
        Task HandleAsync(Event @event, Principal principal);

        /// <summary>
        /// Handle the query.
        /// </summary>
        Task<IQueryable<T>> HandleAsync<T>(IQuery<T> query, Principal principal)
            where T : class;

        /// <summary>
        /// Try to handle the command.
        /// If successfully handled, return true,
        /// otherwise, false.
        /// </summary>
        Task<bool> TryHandleAsync(Command command, Principal principal);

        /// <summary>
        /// The types of messages handled by this message handler.
        /// </summary>
        /// <remarks>
        /// The types must implement <see cref="IMessage"/>.
        /// </remarks>
        IEnumerable<Type> MessageTypes { get; }
    }
}