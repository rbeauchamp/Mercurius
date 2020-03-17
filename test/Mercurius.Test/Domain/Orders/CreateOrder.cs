using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Mercurius.Test.Domain.Orders
{
    /// <summary>
    /// A command where <see cref="ValidateAsync"/>
    /// returns a validation error if <see cref="ReturnValidationError"/> is true.
    /// </summary>
    public class CreateOrder : Command
    {
        public bool ReturnValidationError { get; set; }

        /// <inheritdoc />
        public override Task<IEnumerable<ValidationResult>> ValidateAsync(ValidationContext validationContext)
        {
            return ReturnValidationError
                ? Task.FromResult((IEnumerable<ValidationResult>) new List<ValidationResult>
                {
                    new ValidationResult("The order is not valid")
                })
                : Task.FromResult((IEnumerable<ValidationResult>) new List<ValidationResult>());
        }
    }
}