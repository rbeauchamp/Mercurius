using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Mercurius.Validations
{
    public class ValidateObjectAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                // we cannot validate the properties of the given object when it is null
                return ValidationResult.Success;
            }

            var dictionary = value as IDictionary;
            if (dictionary != null)
            {
                return ValidateDictionary(dictionary, validationContext);
            }

            var enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                return ValidateEnumerable(enumerable.OfType<object>(), validationContext);
            }

            return ValidateObject(value, validationContext);
        }

        private static ValidationResult ValidateDictionary(IDictionary dictionary, ValidationContext validationContext)
        {
            var compositeResults = new CompositeValidationResult($"Validation for {validationContext.DisplayName} failed");

            var resultForKeys = ValidateEnumerable(dictionary.Keys.OfType<object>(), validationContext);
            if (resultForKeys != ValidationResult.Success)
            {
                compositeResults.AddResult(resultForKeys);
            }

            var resultForValues = ValidateEnumerable(dictionary.Values.OfType<object>(), validationContext);
            if (resultForValues != ValidationResult.Success)
            {
                compositeResults.AddResult(resultForValues);
            }


            return compositeResults.Results.Any() ? compositeResults : ValidationResult.Success;
        }

        private static ValidationResult ValidateEnumerable(IEnumerable<object> enumerable, ValidationContext validationContext)
        {
            var results = enumerable.Select(item => ValidateObject(item, new ValidationContext(item, validationContext, validationContext.Items)))
                .Where(result => result != ValidationResult.Success)
                .ToList();

            if (!results.Any())
            {
                return ValidationResult.Success;
            }

            var compositeResults = new CompositeValidationResult($"Validation for {validationContext.DisplayName} failed");
            results.ForEach(compositeResults.AddResult);

            return compositeResults;
        }

        private static ValidationResult ValidateObject(object value, ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(value, validationContext, validationContext.Items);

            Validator.TryValidateObject(value, context, results, true);
            if (results.Count == 0)
            {
                return ValidationResult.Success;
            }

            var compositeResults = new CompositeValidationResult($"Validation for {validationContext.DisplayName} failed");
            results.ForEach(compositeResults.AddResult);

            return compositeResults;
        }
    }
}