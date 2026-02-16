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

internal sealed class UpdateUserProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("users/profile", async (Request request, ClaimsPrincipal claims, ISender sender) =>
        {
            Result result = await sender.Send(new UpdateUserCommand(
                Guid.Parse(claims.GetIdentityId()),
                request.Bio,
                request.Latitude,
                request.Longitude,
                request.City,
                request.Country,
                request.Interests));

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ModifyUser)
        .WithTags(Tags.Users);
    }

    internal sealed class Request
    {
        public string? Bio { get; init; }
        public double? Latitude { get; init; }
        public double? Longitude { get; init; }
        public string? City { get; init; }
        public string? Country { get; init; }
        public string? Interests { get; init; }
    }
}
