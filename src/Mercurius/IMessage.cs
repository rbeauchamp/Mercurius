using System.ComponentModel.DataAnnotations;

namespace Mercurius
{
    public interface IMessage : IValidatableObject, IAsyncValidatableObject
    {
    }
}