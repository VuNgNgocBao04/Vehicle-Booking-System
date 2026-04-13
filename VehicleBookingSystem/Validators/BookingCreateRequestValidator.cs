using FluentValidation;
using VehicleBookingSystem.Contracts.Bookings;
using VehicleBookingSystem.Resources;

namespace VehicleBookingSystem.Validators;

public sealed class BookingCreateRequestValidator : AbstractValidator<BookingCreateRequest>
{
    public BookingCreateRequestValidator()
    {
        RuleFor(request => request.PickupDateTime)
            .GreaterThan(DateTime.UtcNow.Date.AddDays(-1))
            .WithMessage(ValidationMessages.BookingPickupDateInvalid);

        RuleFor(request => request.ReturnDateTime)
            .GreaterThan(request => request.PickupDateTime)
            .WithMessage(ValidationMessages.BookingReturnAfterPickup);

        RuleFor(request => request.PickupLocation)
            .NotEmpty().WithMessage(ValidationMessages.PickupLocationRequired);

        RuleFor(request => request.DropoffLocation)
            .NotEmpty().WithMessage(ValidationMessages.DropoffLocationRequired);
    }
}
