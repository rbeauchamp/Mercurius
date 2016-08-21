using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mercurius.Validations
{
    public class CompositeValidationResult : ValidationResult
    {
        private readonly List<ValidationResult> _validationResults = new List<ValidationResult>();

        public CompositeValidationResult(string errorMessage)
            : base(errorMessage)
        {
        }

        public CompositeValidationResult(string errorMessage, IEnumerable<string> memberNames)
            : base(errorMessage, memberNames)
        {
        }

        protected CompositeValidationResult(ValidationResult validationResult)
            : base(validationResult)
        {
        }

        public List<ValidationResult> Results
        {
            get { return _validationResults; }
        }

        public void AddResult(ValidationResult validationResult)
        {
            _validationResults.Add(validationResult);
        }
    }
}