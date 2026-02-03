using System.Globalization;
using eMeetup.Common.Domain;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Users.Application.Users.RegisterUser;
using eMeetup.Modules.Users.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
                request.Username,
                request.DateOfBirth,
                request.Gender));

                return result.Match(
                    userId => Results.Ok(new { UserId = userId, Message = "User registered successfully" }),
                    error => ApiResults.Problem(error)
                );

        })
    .DisableAntiforgery()
    .AllowAnonymous()
    .WithTags(Tags.Users);
    }


    internal sealed class Request
    {
        public string Email { get; init; }

        public string Password { get; init; }

        public string Username { get; init; }

        public DateTime DateOfBirth { get; init; }

        public Gender Gender { get; init; }
    }
}
