using eMeetup.Common.Domain;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Users.Application.Users.GetUser;
using eMeetup.Modules.Users.Application.Users.Photos.AddPhoto;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Users.Presentation.Users.Photos;

internal sealed class AddPhoto : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/photos", async (
            IFormFile file,
            bool? isPrimary,
            ISender sender,
            CancellationToken ct) =>
        {
            await using var stream = file.OpenReadStream();

            Result<Guid> result = await sender.Send(new AddUserPhotoCommand(
                FileStream: stream,
                FileName: file.FileName,
                ContentType: file.ContentType,
                IsPrimary: isPrimary ?? false), ct);

            return result.Match(
                photoId => Results.Ok(new
                {
                    PhotoId = photoId,
                    Message = "Photo uploaded successfully",
                    IsPrimary = isPrimary ?? false
                }),
                error => ApiResults.Problem(error));
        })
        .DisableAntiforgery()
        .RequireAuthorization()
        .WithTags(Tags.Users)
        .WithName("AddUserPhoto");
    }
}
