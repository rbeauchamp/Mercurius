using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Mercurius.Validations
{
    /// <summary>
    /// </summary>
    public static class HttpValidationExtensions
    {
        /// <summary>Populates the model state with any validation errors.</summary>
        /// <param name="instance"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="modelState"></param>
        /// <param name="principal"></param>
        public static async Task ValidateAsync(this IAsyncValidatableObject instance, IServiceProvider serviceProvider, ModelStateDictionary modelState, IPrincipal principal)
        {
            await instance.IsValidAsync(serviceProvider, modelState, principal.CreateItems()).ConfigureAwait(false);
        }

        /// <summary>
        /// Determines whether this instance is valid.
        /// </summary>
        /// <param name="instance">The instance to validate.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="modelState">State of the model.</param>
        /// <param name="items">The dictionary of key/value pairs to associate with this context.</param>
        public static async Task<bool> IsValidAsync(this IAsyncValidatableObject instance, IServiceProvider serviceProvider, ModelStateDictionary modelState, IDictionary<object, object> items)
        {
            if (modelState is null)
            {
                throw new ArgumentNullException(nameof(modelState));
            }

            var validationResults = new List<ValidationResult>();

            var isValid = await AsyncValidator.TryValidateObjectAsync(instance, new ValidationContext(instance, serviceProvider, items), validationResults).ConfigureAwait(false);

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
        public static List<ValidationResult> Flatten(this IEnumerable<ValidationResult> results, List<ValidationResult> flattenedResults = null)
        {
            if (results is null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            flattenedResults = flattenedResults ?? new List<ValidationResult>();

            foreach (var validationResult in results)
            {
                if (validationResult is CompositeValidationResult compositeValidationResult)
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