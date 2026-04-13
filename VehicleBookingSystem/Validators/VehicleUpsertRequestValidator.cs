using FluentValidation;
using VehicleBookingSystem.Contracts.Vehicles;
using VehicleBookingSystem.Resources;

namespace VehicleBookingSystem.Validators;

public sealed class VehicleUpsertRequestValidator : AbstractValidator<VehicleUpsertRequest>
{
    public VehicleUpsertRequestValidator()
    {
        RuleFor(request => request.Code)
            .NotEmpty().WithMessage(ValidationMessages.CodeRequired)
            .MaximumLength(20).WithMessage(ValidationMessages.StringLength20);

        RuleFor(request => request.DailyRate)
            .InclusiveBetween(100000, 100000000)
            .WithMessage(ValidationMessages.DailyRateRange);

        RuleFor(request => request.LicensePlate)
            .Matches("^[0-9]{2}[A-Z]-[0-9]{3}\\.[0-9]{2}$")
            .WithMessage(ValidationMessages.LicensePlatePattern)
            .When(request => !string.IsNullOrWhiteSpace(request.LicensePlate));
    }
}
