using Microsoft.AspNetCore.Hosting;

namespace VehicleBookingSystem.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "VehicleBookingSystem";
    public string Audience { get; set; } = "VehicleBookingSystem.Client";
    public string Key { get; set; } = string.Empty;
    public int ExpireMinutes { get; set; } = 60;

    public string GetSigningKey(IHostEnvironment environment)
    {
        if (!string.IsNullOrWhiteSpace(Key))
        {
            return Key;
        }

        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException("Jwt:Key must be configured through user-secrets or environment variables outside Development.");
        }

        return $"{Issuer}:{Audience}:{environment.ApplicationName}:DevelopmentOnlySigningKey";
    }
}
