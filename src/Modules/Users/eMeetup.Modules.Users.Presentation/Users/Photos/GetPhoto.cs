using eMeetup.Common.Domain;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Users.Application.Users.Photos.GetPhoto;
using eMeetup.Modules.Users.Application.Users.Photos.GetPhotos;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Users.Presentation.Users.Photos;

internal sealed class GetPhoto : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/photos/{photoId:guid}", async (
            Guid photoId,
            ISender sender,
            CancellationToken ct) =>
        {
            Result<UserPhotoDto> result = await sender.Send(new GetUserPhotoQuery(photoId), ct);

            return result.Match(
                photo => Results.Ok(photo),
                error => ApiResults.Problem(error));
        })
        .RequireAuthorization()
        .WithTags(Tags.Users)
        .WithName("GetUserPhoto");
    }
}
