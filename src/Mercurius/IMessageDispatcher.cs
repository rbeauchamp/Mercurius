namespace Mercurius
{
    /// <summary>
    /// Dispatches messages to message handlers.
    /// </summary>
    public interface IMessageDispatcher
    {
        /// <summary>
        /// Dispatches the specified message to message handlers.
        /// </summary>
        /// <param name="message">The message to dispatch.</param>
        void Dispatch(Message message);
    }
}