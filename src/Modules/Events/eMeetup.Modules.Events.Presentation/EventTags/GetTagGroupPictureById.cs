using eMeetup.Common.Domain;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Events.Application.EventTags.GetTagGroups;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Events.Presentation.EventTags;

internal sealed class GetItemPictureById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("tag-groups/{id}/picture", async (int id, ISender sender) =>
        {
            Result<PictureResponse> result = await sender.Send(new GetItemPictureByIdQuery(id));

            if (result.IsFailure)
            {
                return ApiResults.Problem(result);  // Pass the Result object
            }

            return Results.File(result.Value.Content, result.Value.ContentType,
                lastModified: result.Value.LastModified);
        })
        .Produces<byte[]>(StatusCodes.Status200OK, "application/octet-stream",
            "image/png", "image/gif", "image/jpeg", "image/bmp", "image/tiff",
            "image/wmf", "image/jp2", "image/svg+xml", "image/webp")
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithTags(Tags.EventTags);
    }
}
