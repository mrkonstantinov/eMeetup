using eMeetup.Common.Domain;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Users.Application.Users.RegisterUser;
using eMeetup.Modules.Users.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Users.Presentation.Users;

internal sealed class RegisterUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/register", async (Request request, ISender sender) =>
        {
            Result<Guid> result = await sender.Send(new RegisterUserCommand(
                request.Email,
                request.Password,
                request.UserName,
                request.DateOfBirth,
                request.Gender,
                request.Bio,
                request.ProfilePictureUrl,
                request.Location
                ));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .AllowAnonymous()
        .WithTags(Tags.Users);
    }

    internal sealed class Request
    {
        public string Email { get; init; }

        public string Password { get; init; }

        public string UserName { get; init; }

        public DateTime DateOfBirth { get; init; }

        public Gender Gender { get; init; }

        public string Bio { get; init; }

        public string ProfilePictureUrl { get; init; }

        public Location Location { get; init; }
    }
}
