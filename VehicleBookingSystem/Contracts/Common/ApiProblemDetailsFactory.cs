using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VehicleBookingSystem.Contracts.Common;

public sealed class ApiProblemDetailsFactory
{
    public ProblemDetails Create(int statusCode, string title, string detail, string? instance = null, string? type = null)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = instance,
            Type = type
        };
    }

    public ValidationProblemDetails CreateValidation(string detail, IDictionary<string, string[]> errors)
    {
        var problem = new ValidationProblemDetails(errors)
        {
            Title = "One or more validation errors occurred.",
            Detail = detail,
            Status = StatusCodes.Status400BadRequest
        };

        return problem;
    }
}