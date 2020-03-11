using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Mercurius.Test.Domain.Customers;
using Mercurius.Test.Domain.Orders;
using Mercurius.Test.Services;
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

            var updateCustomer = new UpdateCustomer
            {
                Categories = new List<CreateOrder>
                {
                    new CreateOrder
                    {
                        ReturnValidationError = true
                    }
                }
            };

            // Act
            var isValid = await updateCustomer.IsValidAsync(serviceProvider);

            // Assert
            isValid.Should().BeFalse();
        }
    }
}
