using FluentValidation;
using VehicleBookingSystem.Contracts.Vehicles;

namespace VehicleBookingSystem.Validators;

public sealed class VehicleUpsertRequestValidator : AbstractValidator<VehicleUpsertRequest>
{
    public VehicleUpsertRequestValidator()
    {
        RuleFor(request => request.Code)
            .NotEmpty().WithMessage("Mã xe là bắt buộc.")
            .MaximumLength(20).WithMessage("Mã xe tối đa 20 ký tự.");

        RuleFor(request => request.DailyRate)
            .InclusiveBetween(100000, 100000000)
            .WithMessage("Giá thuê/ngày không hợp lệ.");

        RuleFor(request => request.LicensePlate)
            .Matches("^[0-9]{2}[A-Z]-[0-9]{3}\\.[0-9]{2}$")
            .WithMessage("Biển số không đúng định dạng (VD: 30A-123.45).")
            .When(request => !string.IsNullOrWhiteSpace(request.LicensePlate));
    }
}
