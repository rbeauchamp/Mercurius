using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mercurius.Validations
{
    public static class ValidationExtensions
    {
        public const string PrincipalKey = "PrincipalKey";

        public static async Task IsValidOrThrowExceptionAsync(this IMessage message, IServiceProvider serviceProvider, IPrincipal principal)
        {
            var validationResults = new List<ValidationResult>();

            var context = new ValidationContext(message, serviceProvider, principal.CreateItems());

            if (!await AsyncValidator.TryValidateObjectAsync(message, context, validationResults).ConfigureAwait(false))
            {
                var errors = new JObject();

                var flattenedValidationResults = validationResults.Flatten();

                foreach (var validationResult in flattenedValidationResults)
                {
                    if (validationResult.MemberNames.Any() && !errors.ContainsKey(validationResult.MemberNames.First()))
                    {
                        errors.Add(validationResult.MemberNames.First(), validationResult.ErrorMessage);
                    }
                    else
                    {
                        errors.Add("", validationResult.ErrorMessage);
                    }
                }

                throw new Exception($"The message of type {message.GetType()} is not valid: {JsonConvert.SerializeObject(errors)}");
            }
        }

        public static Dictionary<object, object> CreateItems(this IPrincipal principal)
        {
            return new Dictionary<object, object> { { PrincipalKey, principal } };
        }

        /// <summary>
        /// Determines whether this instance is valid.
        /// </summary>
        /// <param name="instance">The instance to validate.</param>
        /// <param name="serviceProvider">The service provider.</param>
        [Pure]
        public static bool IsValid(this object instance, IServiceProvider serviceProvider)
        {
            return Validator.TryValidateObject(instance, new ValidationContext(instance, serviceProvider, null), null, true);
        }

        /// <summary>
        /// Determines whether this instance is valid.
        /// </summary>
        /// <param name="instance">The instance to validate.</param>
        /// <param name="serviceProvider">The service provider.</param>
        [Pure]
        public static async Task<bool> IsValidAsync(this object instance, IServiceProvider serviceProvider)
        {
            return await AsyncValidator.TryValidateObjectAsync(instance, new ValidationContext(instance, serviceProvider, null), null, true).ConfigureAwait(false);
        }

        public static ValidationResult CreateValidationError<TSource, TProperty>(this TSource source, Expression<Func<TSource, TProperty>> propertyLambda, string errorMessageFormat)
        {
            var propertyName = GetPropertyInfo(source, propertyLambda).Name;
            return new ValidationResult(string.Format(errorMessageFormat, propertyName), new List<string>{propertyName});
        }

        private static PropertyInfo GetPropertyInfo<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> propertyLambda)
        {
            var type = typeof (TSource);

            if (!(propertyLambda.Body is MemberExpression member))
            {
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.");
            }

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");
            }

            //if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
            if (type != propInfo.DeclaringType && !type.IsSubclassOf(propInfo.DeclaringType))
            {
                throw new ArgumentException($"Expresion '{propertyLambda}' refers to a property that is not from type {type}.");
            }

            return propInfo;
        }
    }
}