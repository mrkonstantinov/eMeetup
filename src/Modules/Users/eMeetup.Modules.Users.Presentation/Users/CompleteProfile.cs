using eMeetup.Common.Domain;
using eMeetup.Common.Presentation.Endpoints;
using eMeetup.Common.Presentation.Results;
using eMeetup.Modules.Users.Application.Users.CompleteProfile;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eMeetup.Modules.Users.Presentation.Users;

internal sealed class CompleteProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/profile/complete", async (Request request, ISender sender) =>
        {
            Result result = await sender.Send(new CompleteProfileCommand(
                request.AvatarUrl,
                request.DateOfBirth,
                request.Bio,
                request.Phone,
                request.Telegram,
                request.Instagram,
                request.City,
                request.Country,
                request.Latitude,
                request.Longitude,
                request.TimeZone,
                request.Gender,
                request.Languages,
                request.Interests));

            return result.Match(
                () => Results.Ok(new
                {
                    Message = "Profile completed successfully"
                }),
                error => ApiResults.Problem(error));
        })
        .DisableAntiforgery()
        .RequireAuthorization(Permissions.ModifyUser)
        .WithTags(Tags.Users);
    }

    internal sealed class Request
    {
        // Личная информация
        public string? AvatarUrl { get; init; }
        public DateTime? DateOfBirth { get; init; }
        public string? Bio { get; init; }

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
        public string? Gender { get; init; }
        public string? Languages { get; init; }

        // Интересы
        public string? Interests { get; init; }
    }
}
