using System;

namespace Mercurius
{
    /// <summary>
    ///     A significant change of state that happened in the past.
    ///     Implementations of this interface should be named as verbs in the past tense.
    ///     <see cref="https://github.com/eventstore/eventstore/wiki/Event-Sourcing-Basics#what-is-a-domain-event" />
    /// </summary>
    public abstract class Event : Message
    {
        /// <summary>
        ///     Gets the unique id of the aggregate root that generated this event.
        /// </summary>
        public Guid? AggregateRootId { get; set; }
    }
}