using eMeetup.Common.Domain;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Users.Application.Users.Photos.GetPhotos;
using eMeetup.Modules.Users.Application.Users.Photos.GetPhotosById;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Users.Presentation.Users.Photos;

internal sealed class GetUserPhotosById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{userId:guid}/photos", async (
            Guid userId,
            ISender sender,
            CancellationToken ct) =>
        {
            Result<UserPhotosResponse> result = await sender.Send(
                new GetUserPhotosByIdQuery(userId), ct);

            return result.Match(
                photos => Results.Ok(photos),
                error => ApiResults.Problem(error));
        })
        .RequireAuthorization()
        .WithTags(Tags.Users)
        .WithName("GetUserPhotosById");
    }
}
