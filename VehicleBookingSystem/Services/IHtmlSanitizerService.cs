namespace VehicleBookingSystem.Services;

public interface IHtmlSanitizerService
{
    string Sanitize(string? html);
}
