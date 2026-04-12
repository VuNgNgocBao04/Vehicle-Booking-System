using System.ComponentModel.DataAnnotations;

namespace VehicleBookingSystem.Validation;

[AttributeUsage(AttributeTargets.Property)]
public sealed class MinimumAgeAttribute : ValidationAttribute
{
    private readonly int _minimumAge;

    public MinimumAgeAttribute(int minimumAge)
    {
        _minimumAge = minimumAge;
        ErrorMessage = $"Bạn phải đủ {_minimumAge} tuổi.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not DateTime birthday)
        {
            return ValidationResult.Success;
        }

        var today = DateTime.Today;
        var age = today.Year - birthday.Year;
        if (birthday.Date > today.AddYears(-age))
        {
            age--;
        }

        return age >= _minimumAge ? ValidationResult.Success : new ValidationResult(ErrorMessage);
    }
}
