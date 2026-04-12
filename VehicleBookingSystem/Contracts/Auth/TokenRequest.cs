using System.ComponentModel.DataAnnotations;
using VehicleBookingSystem.Resources;

namespace VehicleBookingSystem.Contracts.Auth;

public sealed class TokenRequest
{
    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.Required))]
    [EmailAddress(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.EmailInvalid))]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = nameof(ValidationMessages.Required))]
    public string Password { get; init; } = string.Empty;
}
