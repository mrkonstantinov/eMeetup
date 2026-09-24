using eMeetup.Common.Domain;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Users.Application.Users.Photos.GetPhotos;
using eMeetup.Modules.Users.Application.Users.Photos.GetPrimary;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Users.Presentation.Users.Photos;

internal sealed class GetPrimaryPhoto : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/photos/primary", async (ISender sender, CancellationToken ct) =>
        {
            Result<UserPhotoDto?> result = await sender.Send(new GetPrimaryPhotoQuery(), ct);

            return result.Match(
                photo => photo is null
                    ? Results.NotFound(new { Message = "Primary photo not found" })
                    : Results.Ok(photo),
                error => ApiResults.Problem(error));
        })
        .RequireAuthorization()
        .WithTags(Tags.Users)
        .WithName("GetPrimaryPhoto");
    }
}
