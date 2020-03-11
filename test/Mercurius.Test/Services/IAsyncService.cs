using System.Threading.Tasks;

namespace Mercurius.Test.Services
{
    public interface IAsyncService
    {
        Task InvokeAsync();
    }
}