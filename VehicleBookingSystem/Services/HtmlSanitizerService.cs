using Ganss.Xss;

namespace VehicleBookingSystem.Services;

public sealed class HtmlSanitizerService : IHtmlSanitizerService
{
    private readonly HtmlSanitizer _sanitizer;

    public HtmlSanitizerService()
    {
        _sanitizer = new HtmlSanitizer();
        _sanitizer.AllowedTags.Add("img");
        _sanitizer.AllowedAttributes.Add("class");
    }

    public string Sanitize(string? html)
    {
        return string.IsNullOrWhiteSpace(html) ? string.Empty : _sanitizer.Sanitize(html);
    }
}
