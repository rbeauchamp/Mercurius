﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Mercurius.Validations
{
    public static class AsyncValidator
    {
        private static readonly ValidationAttributeStore Store = ValidationAttributeStore.Instance;

        public static async Task<bool> TryValidatePropertyAsync(object value, ValidationContext validationContext, ICollection<ValidationResult> validationResults)
        {
            if (validationContext is null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }

            var propertyType = Store.GetPropertyType(validationContext);
            EnsureValidPropertyType(propertyType, value);
            var flag = true;
            var breakOnFirstError = validationResults == null;
            var validationAttributes = Store.GetPropertyValidationAttributes(validationContext);
            foreach (var validationError in await GetValidationErrorsAsync(value, validationContext, validationAttributes, breakOnFirstError).ConfigureAwait(false))
            {
                flag = false;
                validationResults?.Add(validationError.ValidationResult);
            }
            return flag;
        }

        public static async Task<bool> TryValidateObjectAsync(object instance, ValidationContext validationContext, ICollection<ValidationResult> validationResults)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            if (validationContext != null && instance != validationContext.ObjectInstance)
            {
                throw new ArgumentException(ExceptionMessages.InstanceMustMatchValidationContextInstance, nameof(instance));
            }
            var flag = true;
            var breakOnFirstError = validationResults == null;
            foreach (var validationError in await GetObjectValidationErrorsAsync(instance, validationContext, breakOnFirstError).ConfigureAwait(false))
            {
                flag = false;
                validationResults?.Add(validationError.ValidationResult);
            }
            return flag;
        }

        public static async Task<bool> TryValidateValueAsync(object value, ValidationContext validationContext, ICollection<ValidationResult> validationResults, IEnumerable<ValidationAttribute> validationAttributes)
        {
            var flag = true;
            var breakOnFirstError = validationResults == null;
            foreach (var validationError in await GetValidationErrorsAsync(value, validationContext, validationAttributes, breakOnFirstError).ConfigureAwait(false))
            {
                flag = false;
                validationResults?.Add(validationError.ValidationResult);
            }
            return flag;
        }

        public static async Task ValidatePropertyAsync(object value, ValidationContext validationContext)
        {
            if (validationContext is null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }

            var propertyType = Store.GetPropertyType(validationContext);
            EnsureValidPropertyType(propertyType, value);
            var validationAttributes = Store.GetPropertyValidationAttributes(validationContext);
            var validationError = (await GetValidationErrorsAsync(value, validationContext, validationAttributes, false).ConfigureAwait(false)).FirstOrDefault();
            validationError?.ThrowValidationException();
        }

        public static async Task ValidateObjectAsync(object instance, ValidationContext validationContext)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            if (validationContext == null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }
            if (instance != validationContext.ObjectInstance)
            {
                throw new ArgumentException(ExceptionMessages.InstanceMustMatchValidationContextInstance, nameof(instance));
            }

            var validationError = (await GetObjectValidationErrorsAsync(instance, validationContext, false).ConfigureAwait(false)).FirstOrDefault();

            validationError?.ThrowValidationException();
        }

        public static async Task ValidateValueAsync(object value, ValidationContext validationContext, IEnumerable<ValidationAttribute> validationAttributes)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }
            var validationError = (await GetValidationErrorsAsync(value, validationContext, validationAttributes, false).ConfigureAwait(false)).FirstOrDefault();
            validationError?.ThrowValidationException();
        }

        internal static ValidationContext CreateValidationContext(object instance, ValidationContext validationContext)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }
            return new ValidationContext(instance, validationContext, validationContext.Items);
        }

        private static bool CanBeAssigned(Type destinationType, object value)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException(nameof(destinationType));
            }
            if (value != null)
            {
                return destinationType.IsInstanceOfType(value);
            }
            if (!destinationType.GetTypeInfo().IsValueType)
            {
                return true;
            }
            if (destinationType.GetTypeInfo().IsGenericType)
            {
                return destinationType.GetGenericTypeDefinition() == typeof (Nullable<>);
            }
            return false;
        }

        private static void EnsureValidPropertyType(Type propertyType, object value)
        {
            if (!CanBeAssigned(propertyType, value))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Validator property value wrong type"), nameof(value));
            }
        }

        private static async Task<IEnumerable<ValidationError>> GetObjectValidationErrorsAsync(object instance, ValidationContext validationContext, bool breakOnFirstError)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            if (validationContext == null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }
            var list = new List<ValidationError>();
            list.AddRange(await GetObjectPropertyValidationErrorsAsync(instance, validationContext, breakOnFirstError).ConfigureAwait(false));
            if (list.Any())
            {
                return list;
            }
            var validationAttributes = Store.GetTypeValidationAttributes(validationContext);
            list.AddRange(await GetValidationErrorsAsync(instance, validationContext, validationAttributes, breakOnFirstError).ConfigureAwait(false));
            if (list.Any())
            {
                return list;
            }

            if (instance is IAsyncValidatableObject validatableObject)
            {
                list.AddRange((await validatableObject.ValidateAsync(validationContext).ConfigureAwait(false)).Where(r => r != ValidationResult.Success).Select(validationResult => new ValidationError(null, instance, validationResult)));
            }
            return list;
        }

        private static async Task<IEnumerable<ValidationError>> GetObjectPropertyValidationErrorsAsync(object instance, ValidationContext validationContext, bool breakOnFirstError)
        {
            var propertyValues = GetPropertyValues(instance, validationContext);
            var list = new List<ValidationError>();
            foreach (var keyValuePair in propertyValues)
            {
                var validationAttributes = Store.GetPropertyValidationAttributes(keyValuePair.Key);
                //if (validateAllProperties)
                //{
                    list.AddRange(await GetValidationErrorsAsync(keyValuePair.Value, keyValuePair.Key, validationAttributes, breakOnFirstError).ConfigureAwait(false));
                //}
                //else
                //{
                //    var attributes = validationAttributes.ToList();

                //    if (attributes.FirstOrDefault(a => a is RequiredAttribute) is RequiredAttribute requiredAttribute)
                //    {
                //        var validationResult = requiredAttribute.GetValidationResult(keyValuePair.Value, keyValuePair.Key);
                //        if (validationResult != ValidationResult.Success)
                //        {
                //            list.Add(new ValidationError(requiredAttribute, keyValuePair.Value, validationResult));

                //            if (breakOnFirstError)
                //            {
                //                if (list.Any())
                //                {
                //                    break;
                //                }
                //            }
                //        }
                //    }

                //    if (attributes.FirstOrDefault(a => a is IAsyncValidationAttribute) is IAsyncValidationAttribute asyncValidationAttribute)
                //    {
                //        var validationResult = await asyncValidationAttribute.IsValidAsync(keyValuePair.Value, keyValuePair.Key);
                //        if (validationResult != ValidationResult.Success)
                //        {
                //            list.Add(new ValidationError(asyncValidationAttribute, keyValuePair.Value, validationResult));

                //            if (breakOnFirstError)
                //            {
                //                if (list.Any())
                //                {
                //                    break;
                //                }
                //            }
                //        }
                //    }
                //}

                if (breakOnFirstError)
                {
                    if (list.Any())
                    {
                        break;
                    }
                }
            }
            return list;
        }

        private static ICollection<KeyValuePair<ValidationContext, object>> GetPropertyValues(object instance, ValidationContext validationContext)
        {
            var properties = TypeDescriptor.GetProperties(instance);
            var list = new List<KeyValuePair<ValidationContext, object>>(properties.Count);
            foreach (PropertyDescriptor propertyDescriptor in properties)
            {
                var validationContext1 = CreateValidationContext(instance, validationContext);
                validationContext1.MemberName = propertyDescriptor.Name;
                if (Store.GetPropertyValidationAttributes(validationContext1).Any())
                {
                    list.Add(new KeyValuePair<ValidationContext, object>(validationContext1, propertyDescriptor.GetValue(instance)));
                }
            }
            return list;
        }

        private static async Task<IEnumerable<ValidationError>> GetValidationErrorsAsync(object value, ValidationContext validationContext, IEnumerable<ValidationAttribute> attributes, bool breakOnFirstError)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }
            var list = new List<ValidationError>();
            var validationAttributes = attributes as IList<ValidationAttribute> ?? attributes.ToList();
            var requiredAttribute = validationAttributes.FirstOrDefault(a => a is RequiredAttribute) as RequiredAttribute;
            if (requiredAttribute != null)
            {
                var validationError = await TryValidateAsync(value, validationContext, requiredAttribute).ConfigureAwait(false);

                if (validationError != null)
                {
                    list.Add(validationError);
                    return list;
                }
            }
            foreach (var attribute in validationAttributes)
            {
                if (!Equals(attribute, requiredAttribute))
                {
                    var validationError = await TryValidateAsync(value, validationContext, attribute).ConfigureAwait(false);

                    if (validationError != null)
                    {
                        list.Add(validationError);
                        if (breakOnFirstError)
                        {
                            break;
                        }
                    }
                }
            }
            return list;
        }

        private static async Task<ValidationError> TryValidateAsync(object value, ValidationContext validationContext, ValidationAttribute attribute)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }

            var validationResult = attribute.GetValidationResult(value, validationContext);

            if (validationResult != ValidationResult.Success)
            {
                return new ValidationError(attribute, value, validationResult);
            }

            if (attribute is IAsyncValidationAttribute asyncValidationAttribute)
            {
                var asyncValidationResult = await asyncValidationAttribute.IsValidAsync(value, validationContext).ConfigureAwait(false);
                if (asyncValidationResult != ValidationResult.Success)
                {
                    return new ValidationError(attribute, value, asyncValidationResult);
                }
            }

            return null;
        }

        private class ValidationError
        {
            internal ValidationError(ValidationAttribute validationAttribute, object value, ValidationResult validationResult)
            {
                ValidationAttribute = validationAttribute;
                ValidationResult = validationResult;
                Value = value;
            }

            private object Value { get; }
            private ValidationAttribute ValidationAttribute { get; }
            internal ValidationResult ValidationResult { get; }

            internal void ThrowValidationException()
            {
                throw new ValidationException(ValidationResult, ValidationAttribute, Value);
            }
        }
    }

    internal class ValidationAttributeStore
    {
        private readonly Dictionary<Type, TypeStoreItem> _typeStoreItems = new Dictionary<Type, TypeStoreItem>();

        internal static ValidationAttributeStore Instance { get; } = new ValidationAttributeStore();

        internal IEnumerable<ValidationAttribute> GetTypeValidationAttributes(ValidationContext validationContext)
        {
            EnsureValidationContext(validationContext);
            return GetTypeStoreItem(validationContext.ObjectType).ValidationAttributes;
        }

        internal DisplayAttribute GetTypeDisplayAttribute(ValidationContext validationContext)
        {
            EnsureValidationContext(validationContext);
            return GetTypeStoreItem(validationContext.ObjectType).DisplayAttribute;
        }

        internal IEnumerable<ValidationAttribute> GetPropertyValidationAttributes(ValidationContext validationContext)
        {
            EnsureValidationContext(validationContext);
            return GetTypeStoreItem(validationContext.ObjectType).GetPropertyStoreItem(validationContext.MemberName).ValidationAttributes;
        }

        internal DisplayAttribute GetPropertyDisplayAttribute(ValidationContext validationContext)
        {
            EnsureValidationContext(validationContext);
            return GetTypeStoreItem(validationContext.ObjectType).GetPropertyStoreItem(validationContext.MemberName).DisplayAttribute;
        }

        internal Type GetPropertyType(ValidationContext validationContext)
        {
            EnsureValidationContext(validationContext);
            return GetTypeStoreItem(validationContext.ObjectType).GetPropertyStoreItem(validationContext.MemberName).PropertyType;
        }

        internal bool IsPropertyContext(ValidationContext validationContext)
        {
            EnsureValidationContext(validationContext);
            var typeStoreItem = GetTypeStoreItem(validationContext.ObjectType);
            return typeStoreItem.TryGetPropertyStoreItem(validationContext.MemberName, out var propertyStoreItem);
        }

        private TypeStoreItem GetTypeStoreItem(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            lock (_typeStoreItems)
            {
                if (!_typeStoreItems.TryGetValue(type, out var local0))
                {
                    var local1 = TypeDescriptor.GetAttributes(type).Cast<Attribute>();
                    local0 = new TypeStoreItem(type, local1);
                    _typeStoreItems[type] = local0;
                }
                return local0;
            }
        }

        private static void EnsureValidationContext(ValidationContext validationContext)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }
        }

        private abstract class StoreItem
        {
            protected StoreItem(IEnumerable<Attribute> attributes)
            {
                var enumerable = attributes as IList<Attribute> ?? attributes.ToList();
                ValidationAttributes = enumerable.OfType<ValidationAttribute>();
                DisplayAttribute = enumerable.OfType<DisplayAttribute>().SingleOrDefault();
            }

            internal IEnumerable<ValidationAttribute> ValidationAttributes { get; }
            internal DisplayAttribute DisplayAttribute { get; }
        }

        private class TypeStoreItem : StoreItem
        {
            private readonly object _syncRoot = new object();
            private readonly Type _type;
            private Dictionary<string, PropertyStoreItem> _propertyStoreItems;

            internal TypeStoreItem(Type type, IEnumerable<Attribute> attributes) : base(attributes)
            {
                _type = type;
            }

            internal PropertyStoreItem GetPropertyStoreItem(string propertyName)
            {
                if (!TryGetPropertyStoreItem(propertyName, out var propertyStoreItem))
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "AttributeStore_Unknown_Property"), nameof(propertyName));
                }
                return propertyStoreItem;
            }

            internal bool TryGetPropertyStoreItem(string propertyName, out PropertyStoreItem item)
            {
                if (string.IsNullOrEmpty(propertyName))
                {
                    throw new ArgumentNullException(nameof(propertyName));
                }
                if (_propertyStoreItems == null)
                {
                    lock (_syncRoot)
                    {
                        if (_propertyStoreItems == null)
                        {
                            _propertyStoreItems = CreatePropertyStoreItems();
                        }
                    }
                }
                return _propertyStoreItems.TryGetValue(propertyName, out item);
            }

            private Dictionary<string, PropertyStoreItem> CreatePropertyStoreItems()
            {
                var dictionary = new Dictionary<string, PropertyStoreItem>();
                foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(_type))
                {
                    var propertyStoreItem = new PropertyStoreItem(propertyDescriptor.PropertyType, GetExplicitAttributes(propertyDescriptor).Cast<Attribute>());
                    dictionary[propertyDescriptor.Name] = propertyStoreItem;
                }
                return dictionary;
            }

            private static AttributeCollection GetExplicitAttributes(PropertyDescriptor propertyDescriptor)
            {
                var list = new List<Attribute>(propertyDescriptor.Attributes.Cast<Attribute>());
                var enumerable = TypeDescriptor.GetAttributes(propertyDescriptor.PropertyType).Cast<Attribute>();
                var flag = false;
                foreach (var attribute in enumerable)
                {
                    for (var index = list.Count - 1; index >= 0; --index)
                    {
                        if (ReferenceEquals(attribute, list[index]))
                        {
                            list.RemoveAt(index);
                            flag = true;
                        }
                    }
                }
                if (!flag)
                {
                    return propertyDescriptor.Attributes;
                }
                return new AttributeCollection(list.ToArray());
            }
        }

        private class PropertyStoreItem : StoreItem
        {
            internal PropertyStoreItem(Type propertyType, IEnumerable<Attribute> attributes) : base(attributes)
            {
                PropertyType = propertyType;
            }

            internal Type PropertyType { get; }
        }
    }
}