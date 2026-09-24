using eMeetup.Common.Domain;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Users.Application.Users.Photos.GetPhotos;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Users.Presentation.Users.Photos;

internal sealed class GetPhotos : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/photos", async (ISender sender, CancellationToken ct) =>
        {
            Result<UserPhotosResponse> result = await sender.Send(new GetUserPhotosQuery(), ct);

            return result.Match(
                photos => Results.Ok(photos),
                error => ApiResults.Problem(error));
        })
        .RequireAuthorization()
        .WithTags(Tags.Users)
        .WithName("GetUserPhotos");
    }
}
