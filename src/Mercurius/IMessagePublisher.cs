using System.Threading.Tasks;

namespace Mercurius
{
    /// <summary>
    /// Publishes messages to a service bus.
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// Publishes the given message.
        /// </summary>
        Task PublishAsync(IMessage message);
    }
}