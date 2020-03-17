namespace Mercurius
{
    /// <summary>
    /// A significant change of state that happened in the past.
    /// Derived types should be named as verbs in the past tense.
    /// <see cref="https://github.com/eventstore/eventstore/wiki/Event-Sourcing-Basics#what-is-a-domain-event" />
    /// </summary>
    /// <seealso cref="Message" />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Would be a breaking change for this library.")]
    public abstract class Event : Message
    {
    }
}