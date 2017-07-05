using System;
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
            var results = await messageDispatcher.DispatchAsync(query, new Principal());

            // Assert
            results.Should().NotBeNull();
            results.Should().HaveCount(1);
        }

        [Fact]
        public void It_should_throw_an_exception_if_no_handler_found_for_event()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddTransient<IMessageDispatcher, MessageDispatcher>()
                .AddTransient<IMessageHandler, CustomersDomain>()
                .BuildServiceProvider();

            var @event = new CustomerCreated();
            var messageDispatcher = serviceProvider.GetRequiredService<IMessageDispatcher>();

            // Act
            Func<Task> action = async () => await messageDispatcher.DispatchAsync(@event, new Principal());

            // Assert
            action.ShouldThrow<Exception>();
        }
    }
}
