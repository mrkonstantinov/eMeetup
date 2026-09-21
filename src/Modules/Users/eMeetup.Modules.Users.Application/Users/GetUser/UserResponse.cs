using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Application.Users.GetUser;

public sealed class UserResponse
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Nickname { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool ProfileCompleted { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastActiveAt { get; init; }

    public UserProfileDetails Profile { get; init; } = null!;
}

public sealed class UserProfileDetails
{
    // Личная информация
    public string? AvatarUrl { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public int? Age { get; init; }
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
    public string[] Languages { get; init; } = Array.Empty<string>();

    // Интересы
    public string[] Interests { get; init; } = Array.Empty<string>();

    // Активности
    public ActivityPreferencesResponse ActivityPreferences { get; init; } = null!;

    // Статус
    public AvailabilityStatusResponse AvailabilityStatus { get; init; } = null!;
    public bool IsPublic { get; init; }

    // Верификация
    public bool IsEmailVerified { get; init; }
    public bool IsPhoneVerified { get; init; }

    // Фото
    public UserPhotoResponse[] Photos { get; init; } = Array.Empty<UserPhotoResponse>();
    public string? ProfileImageUrl { get; init; }
}

public sealed class UserPhotoResponse
{
    public Guid Id { get; init; }
    public string Url { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public bool IsPrimary { get; init; }
    public int DisplayOrder { get; init; }
    public DateTime UploadedAt { get; init; }
}

public sealed class ActivityPreferencesResponse
{
    public string[] PreferredActivities { get; init; } = Array.Empty<string>();
    public string[] ActivityLevels { get; init; } = Array.Empty<string>();
    public string? PreferredTimeOfDay { get; init; }
    public string[] PreferredDays { get; init; } = Array.Empty<string>();
    public int? MaxDistanceKm { get; init; }
    public int? MinParticipants { get; init; }
    public int? MaxParticipants { get; init; }
}

public sealed class AvailabilityStatusResponse
{
    public string Type { get; init; } = string.Empty;
    public DateTime? AvailableFrom { get; init; }
    public DateTime? AvailableUntil { get; init; }
    public string? StatusMessage { get; init; }
}
