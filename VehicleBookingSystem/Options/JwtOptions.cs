namespace VehicleBookingSystem.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "VehicleBookingSystem";
    public string Audience { get; set; } = "VehicleBookingSystem.Client";
    public string Key { get; set; } = "VehicleBookingSystem-Replace-This-With-A-Strong-Key-2026";
    public int ExpireMinutes { get; set; } = 60;
}
