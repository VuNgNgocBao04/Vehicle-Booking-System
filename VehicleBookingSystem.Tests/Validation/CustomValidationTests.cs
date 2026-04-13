using System.ComponentModel.DataAnnotations;
using System.Globalization;
using VehicleBookingSystem.Validation;

namespace VehicleBookingSystem.Tests.Validation;

public class CustomValidationTests
{
    [Fact]
    public void MinimumAgeAttribute_ReturnsError_WhenAgeUnder18()
    {
        using var cultureScope = new CultureScope("vi-VN");

        var model = new RegisterAgeModel
        {
            DateOfBirth = DateTime.Today.AddYears(-17)
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        var valid = Validator.TryValidateObject(model, context, results, validateAllProperties: true);

        Assert.False(valid);
        Assert.Contains(results, result => result.ErrorMessage!.Contains("18 tuổi"));
    }

    [Fact]
    public void EndDateAfterStartDate_ReturnsError_WhenDatesInvalid()
    {
        using var cultureScope = new CultureScope("vi-VN");

        var model = new DateRangeModel
        {
            StartDate = DateTime.Today.AddDays(3),
            EndDate = DateTime.Today.AddDays(2)
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        var valid = Validator.TryValidateObject(model, context, results, validateAllProperties: true);

        Assert.False(valid);
        Assert.Contains(results, result => result.ErrorMessage!.Contains("Ngày kết thúc"));
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _originalCulture;
        private readonly CultureInfo _originalUiCulture;

        public CultureScope(string cultureName)
        {
            _originalCulture = CultureInfo.CurrentCulture;
            _originalUiCulture = CultureInfo.CurrentUICulture;

            var culture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _originalCulture;
            CultureInfo.CurrentUICulture = _originalUiCulture;
        }
    }

    private sealed class RegisterAgeModel
    {
        [MinimumAge(18)]
        public DateTime DateOfBirth { get; init; }
    }

    [EndDateAfterStartDate(nameof(StartDate), nameof(EndDate))]
    private sealed class DateRangeModel
    {
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
    }
}
