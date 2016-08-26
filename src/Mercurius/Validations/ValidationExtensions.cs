using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace Mercurius.Validations
{
    public static class ValidationExtensions
    {
        /// <summary>
        ///     Determines whether this instance is valid.
        /// </summary>
        /// <param name="instance">The instance to validate.</param>
        /// <param name="serviceProvider">The service provider.</param>
        [Pure]
        public static bool IsValid(this object instance, IServiceProvider serviceProvider)
        {
            return Validator.TryValidateObject(instance, new ValidationContext(instance, serviceProvider, null), null, true);
        }

        public static ValidationResult CreateValidationError<TSource, TProperty>(this TSource source, Expression<Func<TSource, TProperty>> propertyLambda, string errorMessageFormat)
        {
            var propertyName = GetPropertyInfo(source, propertyLambda).Name;
            return new ValidationResult(string.Format(errorMessageFormat, propertyName), new List<string>{propertyName});
        }

        private static PropertyInfo GetPropertyInfo<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> propertyLambda)
        {
            var type = typeof (TSource);

            var member = propertyLambda.Body as MemberExpression;
            if (member == null)
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