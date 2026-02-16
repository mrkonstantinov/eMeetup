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

internal sealed class UploadUserPhotos : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("users/photos", async (HttpContext context, ClaimsPrincipal claims, ISender sender) =>
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

                // Get photos
                var photos = form.Files.Where(f => f.Name == "Photos").ToList();

                // Create command
                var command = new UploadUserPhotosCommand(
                    identity,                    
                    photos.Any() ? photos : null
                );

                Result result = await sender.Send(command);

                return result.Match(Results.NoContent, ApiResults.Problem);
            }
            catch (Exception ex)
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<UpdateUserProfile>>();
                logger.LogError(ex, "Unhandled exception in user photos upload");

                return Results.Problem(
                    detail: "An internal server error occurred",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        })
        .RequireAuthorization(Permissions.ModifyUser)
        .WithTags(Tags.Users);
    }
}
