using System;
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

        public static bool TryValidateProperty(object value, ValidationContext validationContext, ICollection<ValidationResult> validationResults)
        {
            var propertyType = Store.GetPropertyType(validationContext);
            EnsureValidPropertyType(propertyType, value);
            var flag = true;
            var breakOnFirstError = validationResults == null;
            var validationAttributes = Store.GetPropertyValidationAttributes(validationContext);
            foreach (var validationError in GetValidationErrors(value, validationContext, validationAttributes, breakOnFirstError))
            {
                flag = false;
                validationResults?.Add(validationError.ValidationResult);
            }
            return flag;
        }

        public static async Task<bool> TryValidateObjectAsync(object instance, ValidationContext validationContext, ICollection<ValidationResult> validationResults)
        {
            return await TryValidateObjectAsync(instance, validationContext, validationResults, false);
        }

        public static async Task<bool> TryValidateObjectAsync(object instance, ValidationContext validationContext, ICollection<ValidationResult> validationResults, bool validateAllProperties)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            if (validationContext != null && instance != validationContext.ObjectInstance)
            {
                throw new ArgumentException("InstanceMustMatchValidationContextInstance", nameof(instance));
            }
            var flag = true;
            var breakOnFirstError = validationResults == null;
            foreach (var validationError in await GetObjectValidationErrorsAsync(instance, validationContext, validateAllProperties, breakOnFirstError))
            {
                flag = false;
                validationResults?.Add(validationError.ValidationResult);
            }
            return flag;
        }

        public static bool TryValidateValue(object value, ValidationContext validationContext, ICollection<ValidationResult> validationResults, IEnumerable<ValidationAttribute> validationAttributes)
        {
            var flag = true;
            var breakOnFirstError = validationResults == null;
            foreach (var validationError in GetValidationErrors(value, validationContext, validationAttributes, breakOnFirstError))
            {
                flag = false;
                validationResults?.Add(validationError.ValidationResult);
            }
            return flag;
        }

        public static void ValidateProperty(object value, ValidationContext validationContext)
        {
            var propertyType = Store.GetPropertyType(validationContext);
            EnsureValidPropertyType(propertyType, value);
            var validationAttributes = Store.GetPropertyValidationAttributes(validationContext);
            var validationError = GetValidationErrors(value, validationContext, validationAttributes, false).FirstOrDefault();
            validationError?.ThrowValidationException();
        }

        public static async Task ValidateObjectAsync(object instance, ValidationContext validationContext)
        {
            await ValidateObjectAsync(instance, validationContext, false);
        }

        public static async Task ValidateObjectAsync(object instance, ValidationContext validationContext, bool validateAllProperties)
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
                throw new ArgumentException("InstanceMustMatchValidationContextInstance", nameof(instance));
            }
            var validationError = (await GetObjectValidationErrorsAsync(instance, validationContext, validateAllProperties, false)).FirstOrDefault();
            validationError?.ThrowValidationException();
        }

        public static void ValidateValue(object value, ValidationContext validationContext, IEnumerable<ValidationAttribute> validationAttributes)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }
            var validationError = GetValidationErrors(value, validationContext, validationAttributes, false).FirstOrDefault();
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

        private static async Task<IEnumerable<ValidationError>> GetObjectValidationErrorsAsync(object instance, ValidationContext validationContext, bool validateAllProperties, bool breakOnFirstError)
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
            list.AddRange(GetObjectPropertyValidationErrors(instance, validationContext, validateAllProperties, breakOnFirstError));
            if (list.Any())
            {
                return list;
            }
            var validationAttributes = Store.GetTypeValidationAttributes(validationContext);
            list.AddRange(GetValidationErrors(instance, validationContext, validationAttributes, breakOnFirstError));
            if (list.Any())
            {
                return list;
            }
            var validatableObject = instance as IAsyncValidatableObject;
            if (validatableObject != null)
            {
                list.AddRange((await validatableObject.ValidateAsync(validationContext)).Where(r => r != ValidationResult.Success).Select(validationResult => new ValidationError(null, instance, validationResult)));
            }
            return list;
        }

        private static IEnumerable<ValidationError> GetObjectPropertyValidationErrors(object instance, ValidationContext validationContext, bool validateAllProperties, bool breakOnFirstError)
        {
            var propertyValues = GetPropertyValues(instance, validationContext);
            var list = new List<ValidationError>();
            foreach (var keyValuePair in propertyValues)
            {
                var validationAttributes = Store.GetPropertyValidationAttributes(keyValuePair.Key);
                if (validateAllProperties)
                {
                    list.AddRange(GetValidationErrors(keyValuePair.Value, keyValuePair.Key, validationAttributes, breakOnFirstError));
                }
                else
                {
                    var requiredAttribute = validationAttributes.FirstOrDefault(a => a is RequiredAttribute) as RequiredAttribute;
                    if (requiredAttribute != null)
                    {
                        var validationResult = requiredAttribute.GetValidationResult(keyValuePair.Value, keyValuePair.Key);
                        if (validationResult != ValidationResult.Success)
                        {
                            list.Add(new ValidationError(requiredAttribute, keyValuePair.Value, validationResult));
                        }
                    }
                }
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

        private static IEnumerable<ValidationError> GetValidationErrors(object value, ValidationContext validationContext, IEnumerable<ValidationAttribute> attributes, bool breakOnFirstError)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }
            var list = new List<ValidationError>();
            var validationAttributes = attributes as IList<ValidationAttribute> ?? attributes.ToList();
            var requiredAttribute = validationAttributes.FirstOrDefault(a => a is RequiredAttribute) as RequiredAttribute;
            ValidationError validationError;
            if (requiredAttribute != null && !TryValidate(value, validationContext, requiredAttribute, out validationError))
            {
                list.Add(validationError);
                return list;
            }
            foreach (var attribute in validationAttributes)
            {
                if (!Equals(attribute, requiredAttribute) && !TryValidate(value, validationContext, attribute, out validationError))
                {
                    list.Add(validationError);
                    if (breakOnFirstError)
                    {
                        break;
                    }
                }
            }
            return list;
        }

        private static bool TryValidate(object value, ValidationContext validationContext, ValidationAttribute attribute, out ValidationError validationError)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException(nameof(validationContext));
            }
            var validationResult = attribute.GetValidationResult(value, validationContext);
            if (validationResult != ValidationResult.Success)
            {
                validationError = new ValidationError(attribute, value, validationResult);
                return false;
            }
            validationError = null;
            return true;
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
            PropertyStoreItem propertyStoreItem;
            return typeStoreItem.TryGetPropertyStoreItem(validationContext.MemberName, out propertyStoreItem);
        }

        private TypeStoreItem GetTypeStoreItem(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            lock (_typeStoreItems)
            {
                TypeStoreItem local0;
                if (!_typeStoreItems.TryGetValue(type, out local0))
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
                PropertyStoreItem propertyStoreItem;
                if (!TryGetPropertyStoreItem(propertyName, out propertyStoreItem))
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