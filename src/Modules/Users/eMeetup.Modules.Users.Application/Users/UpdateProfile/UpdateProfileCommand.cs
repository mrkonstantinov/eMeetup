using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Users.Application.Users.UpdateUser;

public sealed record UpdateProfileCommand(
    // Личная информация
    string? Bio = null,
    DateTime? DateOfBirth = null,
    string? Gender = null,

    // Контакты
    string? Phone = null,
    string? Telegram = null,
    string? Instagram = null,

    // Местоположение
    string? City = null,
    string? Country = null,
    double? Latitude = null,
    double? Longitude = null,
    string? TimeZone = null,

    // Социальные характеристики
    string? Languages = null,

    // Интересы
    string? Interests = null,

    // Приватность
    bool? IsPublic = null) : ICommand;
