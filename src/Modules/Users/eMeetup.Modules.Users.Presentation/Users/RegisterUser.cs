using System.Globalization;
using eMeetup.Common.Domain;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Users.Application.Users.RegisterUser;
using eMeetup.Modules.Users.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Presentation.Users;

internal sealed class RegisterUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/register", async (HttpContext context, ISender sender) =>
        {
            try
            {
                if (!context.Request.HasFormContentType)
                {
                    return Results.Problem(
                        detail: "Request must be multipart/form-data",
                        statusCode: StatusCodes.Status415UnsupportedMediaType
                    );
                }

                var form = await context.Request.ReadFormAsync();

                // Parse required fields
                var email = form["Email"].FirstOrDefault();
                var password = form["Password"].FirstOrDefault();
                var userName = form["UserName"].FirstOrDefault();
                var dateOfBirthStr = form["DateOfBirth"].FirstOrDefault();
                var genderStr = form["Gender"].FirstOrDefault();


                // Validate required fields
                if (string.IsNullOrEmpty(email) ||
                    string.IsNullOrEmpty(password) ||
                    string.IsNullOrEmpty(userName) ||
                    string.IsNullOrEmpty(dateOfBirthStr) ||
                    string.IsNullOrEmpty(genderStr))
                {
                    return Results.BadRequest("Email, Password, UserName, DateOfBirth, and Gender are required");
                }

                // Parse DateOfBirth with multiple format support
                if (!TryParseDate(dateOfBirthStr, out var dateOfBirth))
                {
                    return Results.BadRequest("Invalid DateOfBirth format. Use YYYY-MM-DD, MM/DD/YYYY, or YYYY-MM-DDTHH:MM:SS");
                }

                // Parse Gender
                if (!Enum.TryParse<Gender>(genderStr, true, out var gender))
                {
                    return Results.BadRequest($"Invalid Gender. Valid values: {string.Join(", ", Enum.GetNames<Gender>())}");
                }

                // Parse optional coordinates
                double? latitude = null;
                double? longitude = null;

                if (!string.IsNullOrEmpty(form["Latitude"].FirstOrDefault()) &&
                    double.TryParse(form["Latitude"].FirstOrDefault(), out var lat))
                {
                    latitude = lat;
                }

                if (!string.IsNullOrEmpty(form["Longitude"].FirstOrDefault()) &&
                    double.TryParse(form["Longitude"].FirstOrDefault(), out var lon))
                {
                    longitude = lon;
                }

                // Get photos
                var photos = form.Files.Where(f => f.Name == "Photos").ToList();

                dateOfBirth = DateTime.UtcNow.AddYears(-30);

                var interests = form["Interests"].FirstOrDefault();

                // Create command
                var command = new RegisterUserCommand(
                    email,
                    password,
                    userName,
                    dateOfBirth,
                    gender,
                    form["Bio"].FirstOrDefault(),
                    latitude,
                    longitude,
                    form["City"].FirstOrDefault(),
                    form["Country"].FirstOrDefault(),
                    interests,
                    photos.Any() ? photos : null
                );

                Result<Guid> result = await sender.Send(command);

                return result.Match(
                    userId => Results.Ok(new { UserId = userId, Message = "User registered successfully" }),
                    error => ApiResults.Problem(error)
                );
            }
            catch (Exception ex)
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<RegisterUser>>();
                logger.LogError(ex, "Unhandled exception in user registration");

                return Results.Problem(
                    detail: "An internal server error occurred",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        })
    .DisableAntiforgery()
    .AllowAnonymous()
    .WithTags(Tags.Users);
    }

    // Helper method for flexible date parsing
    private static bool TryParseDate(string dateString, out DateTime date)
    {
        // Try multiple date formats
        string[] formats = {
        "yyyy-MM-dd",
        "yyyy/MM/dd",
        "MM/dd/yyyy",
        "dd/MM/yyyy",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ssZ",
        "yyyy-MM-dd HH:mm:ss"
    };

        if (DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return true;
        }

        // Fallback to default parsing
        return DateTime.TryParse(dateString, out date);
    }
}
