using eMeetup.Common.Domain;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Users.Application.Users.Photos.GetStats;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Users.Presentation.Users.Photos;

internal sealed class GetPhotosStats : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/photos/stats", async (ISender sender, CancellationToken ct) =>
        {
            Result<PhotoStatsResponse> result = await sender.Send(new GetPhotoStatsQuery(), ct);

            return result.Match(
                stats => Results.Ok(stats),
                error => ApiResults.Problem(error));
        })
        .RequireAuthorization()
        .WithTags(Tags.Users)
        .WithName("GetPhotoStats");
    }
}
