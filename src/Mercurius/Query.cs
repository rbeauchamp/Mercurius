namespace Mercurius
{
    public abstract class Query<TResult> : Message, IQuery<TResult>
    {
    }
}