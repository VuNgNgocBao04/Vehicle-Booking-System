using System.ComponentModel.DataAnnotations;
using VehicleBookingSystem.Validation;

namespace VehicleBookingSystem.Tests.Validation;

public class CustomValidationTests
{
    [Fact]
    public void MinimumAgeAttribute_ReturnsError_WhenAgeUnder18()
    {
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

    [Fact]
    public void EndDateAfterStartDate_ReturnsSuccess_WhenNullableDatesAreMissing()
    {
        var model = new NullableDateRangeModel
        {
            StartDate = null,
            EndDate = null
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        var valid = Validator.TryValidateObject(model, context, results, validateAllProperties: true);

        Assert.True(valid);
        Assert.Empty(results);
    }

    [Fact]
    public void EndDateAfterStartDate_ReturnsSuccess_WhenOnlyOneNullableDateIsMissing()
    {
        var model = new NullableDateRangeModel
        {
            StartDate = DateTime.Today,
            EndDate = null
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        var valid = Validator.TryValidateObject(model, context, results, validateAllProperties: true);

        Assert.True(valid);
        Assert.Empty(results);
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

    [EndDateAfterStartDate(nameof(StartDate), nameof(EndDate))]
    private sealed class NullableDateRangeModel
    {
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
    }
}
