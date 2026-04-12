namespace VehicleBookingSystem.Contracts.Auth;

public sealed class TokenResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
}
