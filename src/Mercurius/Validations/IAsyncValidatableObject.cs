using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Mercurius
{
    public interface IAsyncValidatableObject
    {
        Task<IEnumerable<ValidationResult>> ValidateAsync(ValidationContext validationContext);
    }
}