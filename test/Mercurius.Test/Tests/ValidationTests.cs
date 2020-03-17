using System.Threading.Tasks;
using FluentAssertions;
using Mercurius.Test.Domain.Customers;
using Mercurius.Test.Domain.Orders;
using Mercurius.Validations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Mercurius.Test.Tests
{
    public class ValidationTests
    {
        [Fact]
        public async Task ValidateObjectAttribute_invokes_async_validation_of_child_collections()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var updateCustomer = new UpdateCustomer();
            updateCustomer.Categories.Add(new CreateOrder
            {
                ReturnValidationError = true
            });

            // Act
            var isValid = await updateCustomer.IsValidAsync(serviceProvider).ConfigureAwait(false);

            // Assert
            isValid.Should().BeFalse();
        }
    }
}
