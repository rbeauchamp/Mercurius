using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Mercurius
{
    /// <inheritdoc />
    public abstract class Message : IMessage
    {
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return Enumerable.Empty<ValidationResult>();
        }

        public virtual Task<IEnumerable<ValidationResult>> ValidateAsync(ValidationContext validationContext)
        {
            return Task.FromResult(Enumerable.Empty<ValidationResult>());
        }
    }
}