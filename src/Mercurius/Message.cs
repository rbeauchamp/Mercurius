using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Mercurius
{
    public abstract class Message : IValidatableObject
    {
        /// <summary>
        ///     Determines whether the specified object is valid.
        /// </summary>
        /// <param name="validationContext"> The validation context. </param>
        /// <returns> A collection that holds failed-validation information. </returns>
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return Enumerable.Empty<ValidationResult>();
        }
    }
}