using System.ComponentModel.DataAnnotations;
<<<<<<< HEAD
using VehicleBookingSystem.Resources;
=======
using System.Reflection;
>>>>>>> origin/dev

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
        var startProperty = objectType.GetProperty(_startDateProperty, BindingFlags.Public | BindingFlags.Instance);
        var endProperty = objectType.GetProperty(_endDateProperty, BindingFlags.Public | BindingFlags.Instance);

        if (startProperty is null || endProperty is null)
        {
            return new ValidationResult(ValidationMessages.DateRangeConfigInvalid);
        }

        var startValue = startProperty.GetValue(validationContext.ObjectInstance);
        var endValue = endProperty.GetValue(validationContext.ObjectInstance);

        if (startValue is null || endValue is null)
        {
            return ValidationResult.Success;
        }

        if (startValue is not DateTime startDate || endValue is not DateTime endDate)
        {
            return new ValidationResult("Cấu hình xác thực ngày không hợp lệ.");
        }

        return endDate > startDate
            ? ValidationResult.Success
            : new ValidationResult(ValidationMessages.DateRangeInvalid, [_endDateProperty]);
    }
}
