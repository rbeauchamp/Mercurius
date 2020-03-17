using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Mercurius.Validations
{
    public class ValidateObjectAttribute : ValidationAttribute, IAsyncValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                // we cannot validate the properties of the given object when it is null
                return ValidationResult.Success;
            }

            if (validationContext is null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }

            if (value is IDictionary dictionary)
            {
                return ValidateDictionary(dictionary, validationContext);
            }

            if (value is IEnumerable enumerable)
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

        /// <summary>
        ///     Tests whether the given <paramref name="value" /> is valid with respect to the current
        ///     validation attribute without throwing a <see cref="ValidationException" />
        /// </summary>
        /// <remarks>
        ///     If this method returns <see cref="ValidationResult.Success" />, then validation was successful, otherwise
        ///     an instance of <see cref="ValidationResult" /> will be returned with a guaranteed non-null
        ///     <see cref="ValidationResult.ErrorMessage" />.
        /// </remarks>
        /// <param name="value">The value to validate</param>
        /// <param name="validationContext">
        ///     A <see cref="ValidationContext" /> instance that provides
        ///     context about the validation operation, such as the object and member being validated.
        /// </param>
        /// <returns>
        ///     When validation is valid, <see cref="ValidationResult.Success" />.
        ///     <para>
        ///         When validation is invalid, an instance of <see cref="ValidationResult" />.
        ///     </para>
        /// </returns>
        /// <exception cref="InvalidOperationException"> is thrown if the current attribute is malformed.</exception>
        /// <exception cref="ArgumentNullException">When <paramref name="validationContext" /> is null.</exception>
        public async Task<ValidationResult> GetValidationResultAsync(object value, ValidationContext validationContext)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }

            var result = await IsValidAsync(value, validationContext).ConfigureAwait(false);

            // If validation fails, we want to ensure we have a ValidationResult that guarantees it has an ErrorMessage
            if (result != null)
            {
                if (string.IsNullOrEmpty(result.ErrorMessage))
                {
                    var errorMessage = FormatErrorMessage(validationContext.DisplayName);
                    result = new ValidationResult(errorMessage, result.MemberNames);
                }
            }

            return result;
        }

        public async Task<ValidationResult> IsValidAsync(object value, ValidationContext validationContext)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }

            if (value == null)
            {
                // we cannot validate an object when it is null
                return ValidationResult.Success;
            }

            if (value is IDictionary dictionary)
            {
                return await ValidateDictionaryAsync(dictionary, validationContext).ConfigureAwait(false);
            }

            if (value is IEnumerable enumerable)
            {
                return await ValidateEnumerableAsync(enumerable.OfType<object>(), validationContext).ConfigureAwait(false);
            }

            return await ValidateObjectAsync(value, validationContext).ConfigureAwait(false);
        }

        private static async Task<ValidationResult> ValidateDictionaryAsync(IDictionary dictionary, ValidationContext validationContext)
        {
            var compositeResults = new CompositeValidationResult($"Validation for {validationContext.DisplayName} failed");

            var resultForKeys = await ValidateEnumerableAsync(dictionary.Keys.OfType<object>(), validationContext).ConfigureAwait(false);
            if (resultForKeys != ValidationResult.Success)
            {
                compositeResults.AddResult(resultForKeys);
            }

            var resultForValues = await ValidateEnumerableAsync(dictionary.Values.OfType<object>(), validationContext).ConfigureAwait(false);
            if (resultForValues != ValidationResult.Success)
            {
                compositeResults.AddResult(resultForValues);
            }


            return compositeResults.Results.Any() ? compositeResults : ValidationResult.Success;
        }

        private static async Task<ValidationResult> ValidateEnumerableAsync(IEnumerable<object> enumerable, ValidationContext validationContext)
        {
            var validationTasks = enumerable.Select(async item => await ValidateObjectAsync(item, new ValidationContext(item, validationContext, validationContext.Items)).ConfigureAwait(false));

            var results = await Task.WhenAll(validationTasks).ConfigureAwait(false);

            if (results.All(result => result == ValidationResult.Success))
            {
                return ValidationResult.Success;
            }

            var compositeResults = new CompositeValidationResult($"Validation for {validationContext.DisplayName} failed");

            Array.ForEach(results, result => compositeResults.AddResult(result));

            return compositeResults;
        }

        private static async Task<ValidationResult> ValidateObjectAsync(object value, ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(value, validationContext, validationContext.Items);

            await AsyncValidator.TryValidateObjectAsync(value, context, results).ConfigureAwait(false);

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