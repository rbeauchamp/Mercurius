using System;
using System.ComponentModel.DataAnnotations;

namespace Mercurius.Validations
{
    public class ValidEnumValueAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var enumType = value.GetType();
            var valid = Enum.IsDefined(enumType, value);
            if (!valid)
                return new ValidationResult($"{value} is not a valid value for type {enumType.Name}");
            return ValidationResult.Success;
        }
    }
}