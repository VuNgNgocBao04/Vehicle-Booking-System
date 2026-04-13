using System.ComponentModel.DataAnnotations;
using VehicleBookingSystem.Resources;

namespace VehicleBookingSystem.Validation;

[AttributeUsage(AttributeTargets.Class)]
public sealed class EndDateAfterStartDateAttribute : ValidationAttribute
{
    private readonly string _startDateProperty;
    private readonly string _endDateProperty;

    public EndDateAfterStartDateAttribute(string startDateProperty, string endDateProperty)
    {
        _startDateProperty = startDateProperty;
        _endDateProperty = endDateProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var objectType = validationContext.ObjectType;
        var startProperty = objectType.GetProperty(_startDateProperty);
        var endProperty = objectType.GetProperty(_endDateProperty);

        if (startProperty is null || endProperty is null)
        {
            return new ValidationResult(ValidationMessages.DateRangeConfigInvalid);
        }

        var startValue = startProperty.GetValue(validationContext.ObjectInstance) as DateTime?;
        var endValue = endProperty.GetValue(validationContext.ObjectInstance) as DateTime?;

        if (!startValue.HasValue || !endValue.HasValue)
        {
            return ValidationResult.Success;
        }

        return endValue.Value > startValue.Value
            ? ValidationResult.Success
            : new ValidationResult(ValidationMessages.DateRangeInvalid, [_endDateProperty]);
    }
}
