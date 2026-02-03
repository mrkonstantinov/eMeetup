using System.Globalization;
using System.Security.Claims;
using eMeetup.Common.Domain;
using eMeetup.Common.Infrastructure.Authentication;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Users.Application.Users.RegisterUser;
using eMeetup.Modules.Users.Application.Users.UpdateUser;
using eMeetup.Modules.Users.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Presentation.Users;

internal sealed class UpdateUserProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("users/profile", async (HttpContext context, ClaimsPrincipal claims, ISender sender) =>
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
                var identity = Guid.Parse(claims.GetIdentityId());

                var form = await context.Request.ReadFormAsync();

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


                var interests = form["Interests"].FirstOrDefault();

                // Create command
                var command = new UpdateUserCommand(
                    identity,
                    form["Bio"].FirstOrDefault(),
                    latitude,
                    longitude,
                    form["City"].FirstOrDefault(),
                    form["Country"].FirstOrDefault(),
                    interests,
                    photos.Any() ? photos : null
                );

                Result result = await sender.Send(command);

                return result.Match(Results.NoContent, ApiResults.Problem);
            }
            catch (Exception ex)
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<UpdateUserProfile>>();
                logger.LogError(ex, "Unhandled exception in user registration");

                return Results.Problem(
                    detail: "An internal server error occurred",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        })
        .RequireAuthorization(Permissions.ModifyUser)
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
