using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Users.Application.Users.CompleteProfile;

public sealed record CompleteProfileCommand(
    // Личная информация
    string? AvatarUrl = null,
    DateTime? DateOfBirth = null,
    string? Bio = null,

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
    string? Gender = null,
    string? Languages = null,

    // Интересы
    string? Interests = null) : ICommand;
