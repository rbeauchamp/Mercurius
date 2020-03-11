using System.Threading.Tasks;

namespace Mercurius.Test.Services
{
    class TestAsyncService : IAsyncService
    {
        /// <inheritdoc />
        public Task InvokeAsync()
        {
            return Task.CompletedTask;
        }
    }
}