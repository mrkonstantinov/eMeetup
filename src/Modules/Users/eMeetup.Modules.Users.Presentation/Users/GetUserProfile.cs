using System.Security.Claims;
using eMeetup.Common.Domain;
using eMeetup.Common.Infrastructure.Authentication;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Users.Application.Users.GetUser;
using eMeetup.Modules.Users.Presentation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Users.Presentation.Users;

internal sealed class GetUserProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/profile", async (ISender sender) =>
        {
            Result<UserResponse> result = await sender.Send(new GetUserQuery());

            return result.Match(
                profile => Results.Ok(profile),
                error => ApiResults.Problem(error));
        })
        .RequireAuthorization()
        .WithTags(Tags.Users);
    }
}
