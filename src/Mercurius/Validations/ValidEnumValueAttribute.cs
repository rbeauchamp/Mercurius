using System;
using System.ComponentModel.DataAnnotations;

namespace Mercurius.Validations
{
    public class ValidEnumValueAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                var enumType = value.GetType();
                var valid = Enum.IsDefined(enumType, value);
                return !valid ? new ValidationResult($"{value} is not a valid value for type {enumType.Name}") : ValidationResult.Success;
            }

            return new ValidationResult("The parameter 'value' is null");
        }
    }
}