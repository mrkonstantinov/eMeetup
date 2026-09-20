using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Activities;
using eMeetup.Modules.Users.Domain.Tags;

namespace eMeetup.Modules.Users.Domain.Users;

public class UserProfile : ValueObject
{
    // === ЛИЧНАЯ ИНФОРМАЦИЯ ===
    public string? AvatarUrl { get; }
    public DateTime? DateOfBirth { get; }
    public string? Bio { get; }

    // === КОНТАКТЫ ===
    public string? Phone { get; }
    public string? Telegram { get; }
    public string? Instagram { get; }

    // === МЕСТОПОЛОЖЕНИЕ ===
    public string? City { get; }
    public string? Country { get; }
    public double? Latitude { get; }
    public double? Longitude { get; }
    public string? TimeZone { get; }

    // === СОЦИАЛЬНЫЕ ХАРАКТЕРИСТИКИ ===
    public string? Gender { get; }
    public string? Languages { get; }

    // === ИНТЕРЕСЫ ===
    public string? Interests { get; }

    // === ПРЕДПОЧТЕНИЯ ДЛЯ АКТИВНОСТЕЙ ===
    public ActivityPreferences ActivityPreferences { get; }

    // === СТАТУС ===
    public AvailabilityStatus AvailabilityStatus { get; }
    public bool IsPublic { get; }

    // === ВЕРИФИКАЦИЯ ===
    public bool IsEmailVerified { get; }
    public bool IsPhoneVerified { get; }

    // === МЕТАДАННЫЕ ===
    public DateTime? LastProfileUpdate { get; }

    private UserProfile() { }

    private UserProfile(
        string? avatarUrl,
        DateTime? dateOfBirth,
        string? bio,
        string? phone,
        string? telegram,
        string? instagram,
        string? city,
        string? country,
        double? latitude,
        double? longitude,
        string? timeZone,
        string? gender,
        string? languages,
        string? interests,
        ActivityPreferences activityPreferences,
        AvailabilityStatus availabilityStatus,
        bool isPublic,
        bool isEmailVerified,
        bool isPhoneVerified)
    {
        AvatarUrl = avatarUrl;
        DateOfBirth = dateOfBirth;
        Bio = bio?.Trim();
        Phone = phone?.Trim();
        Telegram = telegram?.Trim();
        Instagram = instagram?.Trim();
        City = city?.Trim();
        Country = country?.Trim();
        Latitude = latitude;
        Longitude = longitude;
        TimeZone = timeZone;
        Gender = gender;
        Languages = languages;
        Interests = interests?.Trim();
        ActivityPreferences = activityPreferences ?? ActivityPreferences.Default();
        AvailabilityStatus = availabilityStatus ?? AvailabilityStatus.Available();
        IsPublic = isPublic;
        IsEmailVerified = isEmailVerified;
        IsPhoneVerified = isPhoneVerified;
        LastProfileUpdate = DateTime.UtcNow;
    }

    // === ФАБРИЧНЫЕ МЕТОДЫ ===

    public static UserProfile CreateMinimal(string nickname, string email)
    {
        if (string.IsNullOrWhiteSpace(nickname))
            throw new ArgumentException("Nickname cannot be empty", nameof(nickname));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        return new UserProfile(
            avatarUrl: null, dateOfBirth: null, bio: null,
            phone: null, telegram: null, instagram: null,
            city: null, country: null, latitude: null, longitude: null, timeZone: null,
            gender: null, languages: null, interests: null,
            activityPreferences: ActivityPreferences.Default(),
            availabilityStatus: AvailabilityStatus.Available(),
            isPublic: true,
            isEmailVerified: false, isPhoneVerified: false);
    }

    public static Result<UserProfile> Create(
        string? avatarUrl = null,
        DateTime? dateOfBirth = null,
        string? bio = null,
        string? phone = null,
        string? telegram = null,
        string? instagram = null,
        string? city = null,
        string? country = null,
        double? latitude = null,
        double? longitude = null,
        string? timeZone = null,
        string? gender = null,
        string? languages = null,
        string? interests = null,
        ActivityPreferences? activityPreferences = null,
        AvailabilityStatus? availabilityStatus = null,
        bool isPublic = true,
        bool isEmailVerified = false,
        bool isPhoneVerified = false)
    {
        // Валидация
        if (bio?.Length > 500)
            return Result.Failure<UserProfile>(ProfileErrors.BioTooLong(500));

        if (dateOfBirth.HasValue)
        {
            var age = CalculateAge(dateOfBirth.Value);
            if (age < 13)
                return Result.Failure<UserProfile>(ProfileErrors.Underage(13));
            if (age > 120)
                return Result.Failure<UserProfile>(ProfileErrors.InvalidDateOfBirth);
        }

        if (!string.IsNullOrEmpty(avatarUrl) && !IsValidUrl(avatarUrl))
            return Result.Failure<UserProfile>(ProfileErrors.InvalidAvatarUrl);

        if (!string.IsNullOrEmpty(phone) && !IsValidPhone(phone))
            return Result.Failure<UserProfile>(ProfileErrors.InvalidPhone);

        if (latitude.HasValue && (latitude < -90 || latitude > 90))
            return Result.Failure<UserProfile>(ProfileErrors.InvalidCoordinates);

        if (longitude.HasValue && (longitude < -180 || longitude > 180))
            return Result.Failure<UserProfile>(ProfileErrors.InvalidCoordinates);

        if (languages != null && !IsValidLanguages(languages))
            return Result.Failure<UserProfile>(ProfileErrors.InvalidLanguage);

        var profile = new UserProfile(
            avatarUrl, dateOfBirth, bio,
            phone, telegram, instagram,
            city, country, latitude, longitude, timeZone,
            gender, languages, interests,
            activityPreferences ?? ActivityPreferences.Default(),
            availabilityStatus ?? AvailabilityStatus.Available(),
            isPublic, isEmailVerified, isPhoneVerified);

        return Result<UserProfile>.Success(profile);
    }

    // === БИЗНЕС-МЕТОДЫ ===

    public bool IsProfileComplete()
    {
        return !string.IsNullOrEmpty(Bio) &&
               !string.IsNullOrEmpty(City) &&
               DateOfBirth.HasValue &&
               !string.IsNullOrEmpty(Interests);
    }

    public bool IsMinimal()
    {
        return string.IsNullOrEmpty(City) &&
               !DateOfBirth.HasValue &&
               string.IsNullOrEmpty(Bio) &&
               string.IsNullOrEmpty(Interests);
    }

    public int GetAge()
    {
        if (!DateOfBirth.HasValue) return 0;
        var today = DateTime.UtcNow;
        var age = today.Year - DateOfBirth.Value.Year;
        if (DateOfBirth.Value.Date > today.AddYears(-age)) age--;
        return age;
    }

    public string[] GetLanguagesArray() =>
        string.IsNullOrEmpty(Languages)
            ? Array.Empty<string>()
            : Languages.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).ToArray();

    public string[] GetInterestsArray() =>
        string.IsNullOrEmpty(Interests)
            ? Array.Empty<string>()
            : Interests.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim()).ToArray();

    public bool SpeaksLanguage(string language)
    {
        if (string.IsNullOrEmpty(Languages)) return false;
        return Languages.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim().ToLower())
            .Contains(language.ToLower());
    }

    public bool IsInAgeRange(int minAge, int maxAge)
    {
        var age = GetAge();
        return age >= minAge && age <= maxAge;
    }

    public bool IsNearLocation(double targetLat, double targetLng, double maxDistanceKm)
    {
        if (!Latitude.HasValue || !Longitude.HasValue) return false;
        var distance = CalculateDistance(Latitude.Value, Longitude.Value, targetLat, targetLng);
        return distance <= maxDistanceKm;
    }

    public bool HasAvatar() => !string.IsNullOrEmpty(AvatarUrl);
    public bool HasInterests() => !string.IsNullOrEmpty(Interests);
    public bool HasLanguages() => !string.IsNullOrEmpty(Languages);
    public bool IsFullyVerified() => IsEmailVerified && IsPhoneVerified;

    // === UPDATE МЕТОДЫ ===

    public UserProfile UpdateAvatar(string? avatarUrl) =>
        new(avatarUrl: avatarUrl, DateOfBirth, Bio, Phone, Telegram, Instagram,
            City, Country, Latitude, Longitude, TimeZone, Gender, Languages, Interests,
            ActivityPreferences, AvailabilityStatus, IsPublic, IsEmailVerified, isPhoneVerified: IsPhoneVerified);

    public UserProfile UpdateBasicInfo(string? bio, DateTime? dateOfBirth, string? gender) =>
        new(AvatarUrl, dateOfBirth ?? DateOfBirth, bio ?? Bio,
            Phone, Telegram, Instagram, City, Country, Latitude, Longitude, TimeZone,
            gender ?? Gender, Languages, Interests,
            ActivityPreferences, AvailabilityStatus, IsPublic, IsEmailVerified, IsPhoneVerified);

    public UserProfile UpdateContacts(string? phone, string? telegram, string? instagram) =>
        new(AvatarUrl, DateOfBirth, Bio,
            phone ?? Phone, telegram ?? Telegram, instagram ?? Instagram,
            City, Country, Latitude, Longitude, TimeZone, Gender, Languages, Interests,
            ActivityPreferences, AvailabilityStatus, IsPublic, IsEmailVerified, IsPhoneVerified);

    public UserProfile UpdateLocation(string? city, string? country, double? latitude, double? longitude, string? timeZone) =>
        new(AvatarUrl, DateOfBirth, Bio, Phone, Telegram, Instagram,
            city ?? City, country ?? Country, latitude ?? Latitude, longitude ?? Longitude, timeZone ?? TimeZone,
            Gender, Languages, Interests,
            ActivityPreferences, AvailabilityStatus, IsPublic, IsEmailVerified, IsPhoneVerified);

    public UserProfile UpdateSocial(string? gender, string? languages) =>
        new(AvatarUrl, DateOfBirth, Bio, Phone, Telegram, Instagram,
            City, Country, Latitude, Longitude, TimeZone,
            gender ?? Gender, languages ?? Languages, Interests,
            ActivityPreferences, AvailabilityStatus, IsPublic, IsEmailVerified, IsPhoneVerified);

    public UserProfile UpdateInterests(string? interests) =>
        new(AvatarUrl, DateOfBirth, Bio, Phone, Telegram, Instagram,
            City, Country, Latitude, Longitude, TimeZone, Gender, Languages, interests,
            ActivityPreferences, AvailabilityStatus, IsPublic, IsEmailVerified, IsPhoneVerified);

    public UserProfile UpdateActivityPreferences(ActivityPreferences preferences) =>
        new(AvatarUrl, DateOfBirth, Bio, Phone, Telegram, Instagram,
            City, Country, Latitude, Longitude, TimeZone, Gender, Languages, Interests,
            preferences ?? ActivityPreferences, AvailabilityStatus, IsPublic, IsEmailVerified, IsPhoneVerified);

    public UserProfile UpdateAvailability(AvailabilityStatus newStatus) =>
        new(AvatarUrl, DateOfBirth, Bio, Phone, Telegram, Instagram,
            City, Country, Latitude, Longitude, TimeZone, Gender, Languages, Interests,
            ActivityPreferences, newStatus ?? AvailabilityStatus, IsPublic, IsEmailVerified, IsPhoneVerified);

    public UserProfile UpdatePrivacy(bool isPublic) =>
        new(AvatarUrl, DateOfBirth, Bio, Phone, Telegram, Instagram,
            City, Country, Latitude, Longitude, TimeZone, Gender, Languages, Interests,
            ActivityPreferences, AvailabilityStatus, isPublic, IsEmailVerified, IsPhoneVerified);

    public UserProfile VerifyEmail() =>
        new(AvatarUrl, DateOfBirth, Bio, Phone, Telegram, Instagram,
            City, Country, Latitude, Longitude, TimeZone, Gender, Languages, Interests,
            ActivityPreferences, AvailabilityStatus, IsPublic, true, IsPhoneVerified);

    public UserProfile VerifyPhone() =>
        new(AvatarUrl, DateOfBirth, Bio, Phone, Telegram, Instagram,
            City, Country, Latitude, Longitude, TimeZone, Gender, Languages, Interests,
            ActivityPreferences, AvailabilityStatus, IsPublic, IsEmailVerified, true);

    public UserProfile MakePublic() => UpdatePrivacy(true);
    public UserProfile MakePrivate() => UpdatePrivacy(false);

    public UserProfile WithDefaultAvatar(string? name = null)
    {
        var displayName = name ?? "User";
        return UpdateAvatar($"https://ui-avatars.com/api/?name={Uri.EscapeDataString(displayName)}&background=random");
    }

    // === PRIVATE HELPERS ===

    private static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.UtcNow;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }

    private static bool IsValidUrl(string url) => Uri.TryCreate(url, UriKind.Absolute, out _);

    private static bool IsValidPhone(string phone)
    {
        var cleaned = new string(phone.Where(char.IsDigit).ToArray());
        return cleaned.Length >= 7 && cleaned.Length <= 15;
    }

    private static bool IsValidLanguages(string languages) =>
        languages.Split(',').All(l => l.Trim().Length >= 2);

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AvatarUrl;
        yield return DateOfBirth;
        yield return Bio;
        yield return Phone;
        yield return Telegram;
        yield return Instagram;
        yield return City;
        yield return Country;
        yield return Latitude;
        yield return Longitude;
        yield return TimeZone;
        yield return Gender;
        yield return Languages;
        yield return Interests;
        yield return ActivityPreferences;
        yield return AvailabilityStatus;
        yield return IsPublic;
        yield return IsEmailVerified;
        yield return IsPhoneVerified;
    }
}

public static class ProfileErrors
{
    // === ОШИБКИ ВАЛИДАЦИИ ЛИЧНОЙ ИНФОРМАЦИИ ===

    public static Error FirstNameRequired =>
        Error.Validation("Users.Profile.FirstNameRequired", "First name is required");

    public static Error InvalidLastName(string lastName) =>
        Error.Validation("Users.Profile.InvalidLastName", $"Last name '{lastName}' is invalid");

    public static Error LastNameTooLong(int maxLength) =>
        Error.Validation("Users.Profile.LastNameTooLong", $"Last name cannot exceed {maxLength} characters");

    public static Error LastNameRequired =>
        Error.Validation("Users.Profile.LastNameRequired", "Last name is required");

    public static Error InvalidNickname(string nickname) =>
        Error.Validation("Users.Profile.InvalidNickname", $"Nickname '{nickname}' is invalid");

    public static Error NicknameTooLong(int maxLength) =>
        Error.Validation("Users.Profile.NicknameTooLong", $"Nickname cannot exceed {maxLength} characters");

    public static Error NicknameTooShort(int minLength) =>
        Error.Validation("Users.Profile.NicknameTooShort", $"Nickname must be at least {minLength} characters");

    public static Error NicknameRequired =>
        Error.Validation("Users.Profile.NicknameRequired", "Nickname is required");

    public static Error BioTooLong(int maxLength) =>
        Error.Validation("Users.Profile.BioTooLong", $"Bio cannot exceed {maxLength} characters");

    public static Error InvalidDateOfBirth =>
        Error.Validation("Users.Profile.InvalidDateOfBirth", "Invalid date of birth");

    public static Error Underage(int minimumAge) =>
        Error.Validation("Users.Profile.Underage", $"User must be at least {minimumAge} years old");

    public static Error InvalidAge =>
        Error.Validation("Users.Profile.InvalidAge", "Invalid age");

    public static Error InvalidAvatarUrl =>
        Error.Validation("Users.Profile.InvalidAvatarUrl", "Invalid avatar URL format");

    public static Error InvalidGender =>
        Error.Validation("Users.Profile.InvalidGender", "Invalid gender specified");

    public static Error DisplayNameTooLong(int maxLength) =>
        Error.Validation("Users.Profile.DisplayNameTooLong", $"Display name cannot exceed {maxLength} characters");

    // === ОШИБКИ ВАЛИДАЦИИ КОНТАКТОВ ===

    public static Error InvalidEmail =>
        Error.Validation("Users.Profile.InvalidEmail", "Invalid email format");

    public static Error EmailRequired =>
        Error.Validation("Users.Profile.EmailRequired", "Email is required");

    public static Error InvalidPhone =>
        Error.Validation("Users.Profile.InvalidPhone", "Invalid phone number format");

    public static Error PhoneTooLong(int maxLength) =>
        Error.Validation("Users.Profile.PhoneTooLong", $"Phone number cannot exceed {maxLength} characters");

    public static Error PhoneTooShort(int minLength) =>
        Error.Validation("Users.Profile.PhoneTooShort", $"Phone number must be at least {minLength} characters");

    public static Error InvalidTelegram =>
        Error.Validation("Users.Profile.InvalidTelegram", "Invalid Telegram username format");

    public static Error InvalidInstagram =>
        Error.Validation("Users.Profile.InvalidInstagram", "Invalid Instagram username format");

    public static Error InvalidSocialNetworkUrl =>
        Error.Validation("Users.Profile.InvalidSocialNetworkUrl", "Invalid social network URL format");

    // === ОШИБКИ ВАЛИДАЦИИ МЕСТОПОЛОЖЕНИЯ ===

    public static Error InvalidCity =>
        Error.Validation("Users.Profile.InvalidCity", "Invalid city name");

    public static Error CityTooLong(int maxLength) =>
        Error.Validation("Users.Profile.CityTooLong", $"City name cannot exceed {maxLength} characters");

    public static Error CityRequired =>
        Error.Validation("Users.Profile.CityRequired", "City is required for profile completion");

    public static Error InvalidCountry =>
        Error.Validation("Users.Profile.InvalidCountry", "Invalid country name");

    public static Error CountryTooLong(int maxLength) =>
        Error.Validation("Users.Profile.CountryTooLong", $"Country name cannot exceed {maxLength} characters");

    public static Error CountryRequired =>
        Error.Validation("Users.Profile.CountryRequired", "Country is required for profile completion");

    public static Error InvalidCoordinates =>
        Error.Validation("Users.Profile.InvalidCoordinates", "Invalid geographical coordinates");

    public static Error LatitudeOutOfRange =>
        Error.Validation("Users.Profile.LatitudeOutOfRange", "Latitude must be between -90 and 90 degrees");

    public static Error LongitudeOutOfRange =>
        Error.Validation("Users.Profile.LongitudeOutOfRange", "Longitude must be between -180 and 180 degrees");

    public static Error InvalidTimeZone =>
        Error.Validation("Users.Profile.InvalidTimeZone", "Invalid time zone");

    // === ОШИБКИ ВАЛИДАЦИИ СОЦИАЛЬНЫХ ХАРАКТЕРИСТИК ===

    public static Error InvalidLanguage =>
        Error.Validation("Users.Profile.InvalidLanguage", "Invalid language code format. Use comma-separated values (e.g., 'ru,en,fr')");

    public static Error LanguageTooShort =>
        Error.Validation("Users.Profile.LanguageTooShort", "Each language code must be at least 2 characters");

    public static Error InvalidLanguagesFormat =>
        Error.Validation("Users.Profile.InvalidLanguagesFormat", "Invalid languages format. Use comma-separated values (e.g., 'ru,en,fr')");

    public static Error InvalidOccupation =>
        Error.Validation("Users.Profile.InvalidOccupation", "Invalid occupation");

    public static Error OccupationTooLong(int maxLength) =>
        Error.Validation("Users.Profile.OccupationTooLong", $"Occupation cannot exceed {maxLength} characters");

    public static Error InvalidEducation =>
        Error.Validation("Users.Profile.InvalidEducation", "Invalid education");

    public static Error EducationTooLong(int maxLength) =>
        Error.Validation("Users.Profile.EducationTooLong", $"Education cannot exceed {maxLength} characters");

    // === ОШИБКИ ВАЛИДАЦИИ АКТИВНОСТЕЙ ===

    public static Error InvalidActivityType =>
        Error.Validation("Users.Profile.InvalidActivityType", "Invalid activity type");

    public static Error InvalidActivityLevel =>
        Error.Validation("Users.Profile.InvalidActivityLevel", "Invalid activity level");

    public static Error InvalidTimeOfDay =>
        Error.Validation("Users.Profile.InvalidTimeOfDay", "Invalid time of day");

    public static Error InvalidDayOfWeek =>
        Error.Validation("Users.Profile.InvalidDayOfWeek", "Invalid day of week");

    public static Error InvalidMaxDistance =>
        Error.Validation("Users.Profile.InvalidMaxDistance", "Invalid maximum distance");

    public static Error InvalidParticipantCount =>
        Error.Validation("Users.Profile.InvalidParticipantCount", "Invalid participant count");

    public static Error MaxDistanceTooLarge(int maxDistance) =>
        Error.Validation("Users.Profile.MaxDistanceTooLarge", $"Maximum distance cannot exceed {maxDistance} km");

    public static Error MinParticipantsTooSmall(int minParticipants) =>
        Error.Validation("Users.Profile.MinParticipantsTooSmall", $"Minimum participants must be at least {minParticipants}");

    public static Error MaxParticipantsTooLarge(int maxParticipants) =>
        Error.Validation("Users.Profile.MaxParticipantsTooLarge", $"Maximum participants cannot exceed {maxParticipants}");

    public static Error MinParticipantsGreaterThanMax =>
        Error.Validation("Users.Profile.MinParticipantsGreaterThanMax", "Minimum participants cannot exceed maximum participants");

    // === ОШИБКИ СТАТУСА ПРОФИЛЯ ===

    public static Error ProfileNotComplete =>
        Error.Validation("Users.Profile.NotComplete", "Profile is not complete. Please fill all required fields");

    public static Error ProfileAlreadyComplete =>
        Error.Conflict("Users.Profile.AlreadyComplete", "Profile is already complete");

    public static Error CannotUpdateDeletedUser =>
        Error.Problem("Users.Profile.CannotUpdateDeleted", "Cannot update profile of deleted user");

    public static Error CannotUpdateSuspendedUser =>
        Error.Problem("Users.Profile.CannotUpdateSuspended", "Cannot update profile of suspended user");

    public static Error CannotUpdateInactiveUser =>
        Error.Problem("Users.Profile.CannotUpdateInactive", "Cannot update profile of inactive user");

    public static Error ProfileLocked =>
        Error.Problem("Users.Profile.Locked", "Profile is locked for editing");

    public static Error CannotChangeEmail =>
        Error.Problem("Users.Profile.CannotChangeEmail", "Email cannot be changed. Contact support");

    public static Error CannotChangeVerifiedFields =>
        Error.Problem("Users.Profile.CannotChangeVerified", "Verified fields cannot be changed");

    // === ОШИБКИ НЕ НАЙДЕНО ===

    public static Error NotFound(Guid userId) =>
        Error.NotFound("Users.Profile.NotFound", $"Profile for user '{userId}' was not found");

    public static Error AvatarNotFound =>
        Error.NotFound("Users.Profile.AvatarNotFound", "Avatar not found for this user");

    public static Error SocialInfoNotFound =>
        Error.NotFound("Users.Profile.SocialInfoNotFound", "Social information not found for this user");

    public static Error LocationNotFound =>
        Error.NotFound("Users.Profile.LocationNotFound", "Location information not found for this user");

    // === ОШИБКИ ОПЕРАЦИЙ ===

    public static Error ProfileUpdateFailed(string reason) =>
        Error.Failure("Users.Profile.UpdateFailed", $"Profile update failed: {reason}");

    public static Error ProfileCreationFailed(string reason) =>
        Error.Failure("Users.Profile.CreationFailed", $"Profile creation failed: {reason}");

    public static Error AvatarUpdateFailed(string reason) =>
        Error.Failure("Users.Profile.AvatarUpdateFailed", $"Avatar update failed: {reason}");

    public static Error AvatarDeleteFailed(string reason) =>
        Error.Failure("Users.Profile.AvatarDeleteFailed", $"Avatar deletion failed: {reason}");

    public static Error LocationUpdateFailed(string reason) =>
        Error.Failure("Users.Profile.LocationUpdateFailed", $"Location update failed: {reason}");

    public static Error SocialUpdateFailed(string reason) =>
        Error.Failure("Users.Profile.SocialUpdateFailed", $"Social information update failed: {reason}");

    public static Error PreferencesUpdateFailed(string reason) =>
        Error.Failure("Users.Profile.PreferencesUpdateFailed", $"Preferences update failed: {reason}");

    public static Error InvalidOperation(string reason) =>
        Error.Problem("Users.Profile.InvalidOperation", reason);

    public static Error DataIntegrityViolation(string details) =>
        Error.Problem("Users.Profile.DataIntegrityViolation", $"Data integrity violation: {details}");

    // === ОШИБКИ ПРАВ ДОСТУПА ===

    public static Error CannotViewOtherProfile =>
        Error.Forbidden("Users.Profile.CannotViewOther", "You don't have permission to view this user's profile");

    public static Error CannotEditOtherProfile =>
        Error.Forbidden("Users.Profile.CannotEditOther", "You don't have permission to edit this user's profile");

    public static Error CannotViewDeletedProfile =>
        Error.Forbidden("Users.Profile.CannotViewDeleted", "Cannot view profile of deleted user");

    public static Error CannotViewSuspendedProfile =>
        Error.Forbidden("Users.Profile.CannotViewSuspended", "Cannot view profile of suspended user");

    // === ОШИБКИ ВЕРИФИКАЦИИ ===

    public static Error EmailAlreadyVerified =>
        Error.Conflict("Users.Profile.EmailAlreadyVerified", "Email is already verified");

    public static Error PhoneAlreadyVerified =>
        Error.Conflict("Users.Profile.PhoneAlreadyVerified", "Phone is already verified");

    public static Error IdentityAlreadyVerified =>
        Error.Conflict("Users.Profile.IdentityAlreadyVerified", "Identity is already verified");

    public static Error EmailNotVerified =>
        Error.Validation("Users.Profile.EmailNotVerified", "Email is not verified");

    public static Error PhoneNotVerified =>
        Error.Validation("Users.Profile.PhoneNotVerified", "Phone is not verified");

    public static Error IdentityNotVerified =>
        Error.Validation("Users.Profile.IdentityNotVerified", "Identity is not verified");

    public static Error VerificationFailed(string reason) =>
        Error.Failure("Users.Profile.VerificationFailed", $"Verification failed: {reason}");

    public static Error InvalidVerificationCode =>
        Error.Validation("Users.Profile.InvalidVerificationCode", "Invalid verification code");

    public static Error VerificationCodeExpired =>
        Error.Validation("Users.Profile.VerificationCodeExpired", "Verification code has expired");

    public static Error VerificationCodeRequired =>
        Error.Validation("Users.Profile.VerificationCodeRequired", "Verification code is required");

    // === ОШИБКИ ПРИВАТНОСТИ ===

    public static Error ProfileIsPrivate =>
        Error.Forbidden("Users.Profile.IsPrivate", "This profile is private");

    public static Error CannotMakeProfilePrivate =>
        Error.Problem("Users.Profile.CannotMakePrivate", "Cannot make profile private while incomplete");

    public static Error CannotMakeProfilePublic =>
        Error.Problem("Users.Profile.CannotMakePublic", "Cannot make profile public while suspended");

    // === ОШИБКИ МЕТАДАННЫХ ===

    public static Error LastProfileUpdateRequired =>
        Error.Validation("Users.Profile.LastProfileUpdateRequired", "Last profile update is required");

    public static Error ProfileNotUpdatedRecently =>
        Error.Problem("Users.Profile.NotUpdatedRecently", "Profile has not been updated recently");

    // === ОШИБКИ ОГРАНИЧЕНИЙ ===

    public static Error ProfileUpdateLimitExceeded =>
        Error.Problem("Users.Profile.UpdateLimitExceeded", "Profile update limit exceeded. Please try again later");

    public static Error DailyUpdateLimitExceeded(int limit) =>
        Error.Problem("Users.Profile.DailyUpdateLimitExceeded", $"Daily update limit of {limit} exceeded");

    public static Error RateLimitExceeded =>
        Error.Problem("Users.Profile.RateLimitExceeded", "Too many profile operations. Please try again later");
}
