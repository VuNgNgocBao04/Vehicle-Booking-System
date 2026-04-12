using FluentValidation;
using VehicleBookingSystem.Contracts.Bookings;

namespace VehicleBookingSystem.Validators;

public sealed class BookingCreateRequestValidator : AbstractValidator<BookingCreateRequest>
{
    public BookingCreateRequestValidator()
    {
        RuleFor(request => request.PickupDateTime)
            .GreaterThan(DateTime.UtcNow.Date.AddDays(-1))
            .WithMessage("Ngày nhận xe không hợp lệ.");

        RuleFor(request => request.ReturnDateTime)
            .GreaterThan(request => request.PickupDateTime)
            .WithMessage("Ngày trả xe phải sau ngày nhận xe.");

        RuleFor(request => request.PickupLocation)
            .NotEmpty().WithMessage("Điểm nhận xe là bắt buộc.");

        RuleFor(request => request.DropoffLocation)
            .NotEmpty().WithMessage("Điểm trả xe là bắt buộc.");
    }
}
