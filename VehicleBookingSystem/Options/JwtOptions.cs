namespace VehicleBookingSystem.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "VehicleBookingSystem";
    public string Audience { get; set; } = "VehicleBookingSystem.Client";
    public string? Key { get; set; }
    public int ExpireMinutes { get; set; } = 60;
}
