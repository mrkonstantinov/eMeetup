using System.Security.Claims;
using eMeetup.Common.Domain;
using eMeetup.Common.Infrastructure.Authentication;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Users.Application.Users.UpdateUser;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Users.Presentation.Users;

internal sealed class UpdateProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("users/profile", async (Request request, ISender sender) =>
        {
            Result result = await sender.Send(new UpdateProfileCommand(
                request.Bio,
                request.DateOfBirth,
                request.Gender,
                request.Phone,
                request.Telegram,
                request.Instagram,
                request.City,
                request.Country,
                request.Latitude,
                request.Longitude,
                request.TimeZone,
                request.Languages,
                request.Interests,
                request.IsPublic));

            return result.Match(
                () => Results.Ok(new
                {
                    Message = "Profile updated successfully"
                }),
                error => ApiResults.Problem(error));
        })
        .DisableAntiforgery()
        .RequireAuthorization()
        .WithTags(Tags.Users);
    }

    internal sealed class Request
    {
        // Личная информация
        public string? Bio { get; init; }
        public DateTime? DateOfBirth { get; init; }
        public string? Gender { get; init; }

        // Контакты
        public string? Phone { get; init; }
        public string? Telegram { get; init; }
        public string? Instagram { get; init; }

        // Местоположение
        public string? City { get; init; }
        public string? Country { get; init; }
        public double? Latitude { get; init; }
        public double? Longitude { get; init; }
        public string? TimeZone { get; init; }

        // Социальные характеристики
        public string? Languages { get; init; }

        // Интересы
        public string? Interests { get; init; }

        // Приватность
        public bool? IsPublic { get; init; }
    }
}
