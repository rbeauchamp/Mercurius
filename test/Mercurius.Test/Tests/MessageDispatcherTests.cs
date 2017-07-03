using System.Threading.Tasks;
using FluentAssertions;
using Mercurius.Test.Domain.Customers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Mercurius.Test.Tests
{
    public class MessageDispatcherTests
    {
        [Fact]
        public async Task It_should_dispatch_a_query_to_the_correct_message_handler()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddTransient<IMessageDispatcher, MessageDispatcher>()
                .AddTransient<IMessageHandler, CustomersDomain>()
                .BuildServiceProvider();

            var query = new GetCustomers();
            var messageDispatcher = serviceProvider.GetRequiredService<IMessageDispatcher>();

            // Act
            var results = await messageDispatcher.DispatchToSingleAsync(query, new Principal());

            // Assert
            results.Should().NotBeNull();
            results.Should().HaveCount(1);
        }
    }
}
