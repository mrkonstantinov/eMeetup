using System.Security.Claims;
using eMeetup.Common.Domain;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Users.Application.Users.GetUser;
using eMeetup.Modules.Users.Application.Users.GetUsersInterests;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Users.Presentation.InterestTags;

internal sealed class GetTags : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("tags", async (ClaimsPrincipal claims, ISender sender) =>
        {
            Result<IReadOnlyCollection<UsersInterestResponse>> result = await sender.Send(new GetUsersInterestsQuery());

            return result.Match(Results.Ok, ApiResults.Problem);
        })
    .RequireAuthorization(Permissions.GetTags)
    .WithTags(Tags.Users);
    }

}
