namespace Mercurius
{
    /// <summary>
    /// A message that returns a result and does not change the observable state of the system (is free of side effects).
    /// </summary>
    public interface IQuery<TResult> : IMessage
    {
    }
}