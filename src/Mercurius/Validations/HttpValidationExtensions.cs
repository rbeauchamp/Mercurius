using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Mercurius.Validations
{
    /// <summary>
    /// </summary>
    public static class HttpValidationExtensions
    {
        /// <summary>
        /// Determines whether this instance is valid.
        /// </summary>
        /// <param name="instance">The instance to validate.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="modelState">State of the model.</param>
        /// <param name="items">The dictionary of key/value pairs to associate with this context.</param>
        public static async Task<bool> IsValidAsync(this IAsyncValidatableObject instance, IServiceProvider serviceProvider, ModelStateDictionary modelState, IDictionary<object, object> items)
        {
            var validationResults = new List<ValidationResult>();

            var isValid = await AsyncValidator.TryValidateObjectAsync(instance, new ValidationContext(instance, serviceProvider, items), validationResults, true).ConfigureAwait(false);

            var flattenedValidationResults = Flatten(validationResults);

            foreach (var validationResult in flattenedValidationResults)
            {
                if (validationResult.MemberNames.Any())
                {
                    modelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
                }
                modelState.AddModelError("", validationResult.ErrorMessage);
            }

            return isValid;
        }

        /// <summary>
        ///     Determines whether this instance is valid.
        /// </summary>
        /// <param name="instance">The instance to validate.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="modelState">State of the model.</param>
        private static bool IsValid(this IValidatableObject instance, IServiceProvider serviceProvider, ModelStateDictionary modelState)
        {
            var validationResults = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(instance, new ValidationContext(instance, serviceProvider, null), validationResults, true);

            var flattenedValidationResults = Flatten(validationResults);

            foreach (var validationResult in flattenedValidationResults)
            {
                if (validationResult.MemberNames.Any())
                {
                    modelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
                }
                modelState.AddModelError("", validationResult.ErrorMessage);
            }

            return isValid;
        }

        /// <summary>
        ///     Flattens the specified results.
        /// </summary>
        /// <param name="results">The results.</param>
        /// <param name="flattenedResults">The flattened results.</param>
        /// <returns></returns>
        public static List<ValidationResult> Flatten(List<ValidationResult> results, List<ValidationResult> flattenedResults = null)
        {
            flattenedResults = flattenedResults ?? new List<ValidationResult>();

            foreach (var validationResult in results)
            {
                var compositeValidationResult = validationResult as CompositeValidationResult;
                if (compositeValidationResult != null)
                {
                    flattenedResults = Flatten(compositeValidationResult.Results, flattenedResults);
                }
                else
                {
                    flattenedResults.Add(validationResult);
                }
            }

            return flattenedResults;
        }
    }
}