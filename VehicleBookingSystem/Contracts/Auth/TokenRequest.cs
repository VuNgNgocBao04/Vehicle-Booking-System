using System.ComponentModel.DataAnnotations;

namespace VehicleBookingSystem.Contracts.Auth;

public sealed class TokenRequest
{
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    public string Password { get; init; } = string.Empty;
}
