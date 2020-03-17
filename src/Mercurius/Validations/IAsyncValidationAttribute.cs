using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Mercurius.Validations
{
    /// <summary>
    /// Contract for all async validation attributes.
    /// </summary>
    public interface IAsyncValidationAttribute
    {
        Task<ValidationResult> IsValidAsync(object value, ValidationContext validationContext);
    }
}