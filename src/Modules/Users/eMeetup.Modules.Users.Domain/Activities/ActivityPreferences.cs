using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Activities;

public class ActivityPreferences : ValueObject
{
    public IReadOnlySet<ActivityType> PreferredActivities { get; }
    public IReadOnlySet<ActivityLevel> ActivityLevels { get; }
    public TimeOfDay? PreferredTimeOfDay { get; }
    public DayOfWeek[] PreferredDays { get; }
    public int? MaxDistanceKm { get; }
    public int? MinParticipants { get; }
    public int? MaxParticipants { get; }

    private ActivityPreferences() { }

    private ActivityPreferences(
        IEnumerable<ActivityType>? preferredActivities = null,
        IEnumerable<ActivityLevel>? activityLevels = null,
        TimeOfDay? preferredTimeOfDay = null,
        DayOfWeek[]? preferredDays = null,
        int? maxDistanceKm = null,
        int? minParticipants = null,
        int? maxParticipants = null)
    {
        PreferredActivities = (preferredActivities ?? Enumerable.Empty<ActivityType>())
            .ToHashSet().AsReadOnly();
        ActivityLevels = (activityLevels ?? Enumerable.Empty<ActivityLevel>())
            .ToHashSet().AsReadOnly();
        PreferredTimeOfDay = preferredTimeOfDay;
        PreferredDays = preferredDays ?? Array.Empty<DayOfWeek>();
        MaxDistanceKm = maxDistanceKm;
        MinParticipants = minParticipants;
        MaxParticipants = maxParticipants;
    }

    public static ActivityPreferences Default()
    {
        return new ActivityPreferences(
            Array.Empty<ActivityType>(),
            new[] { ActivityLevel.Medium },
            null,
            Array.Empty<DayOfWeek>(),
            50,
            2,
            20);
    }

    public static Result<ActivityPreferences> Create(
    IEnumerable<ActivityType>? preferredActivities = null,
    IEnumerable<ActivityLevel>? activityLevels = null,
    TimeOfDay? preferredTimeOfDay = null,
    DayOfWeek[]? preferredDays = null,
    int? maxDistanceKm = null,
    int? minParticipants = null,
    int? maxParticipants = null)
    {
        // === ВАЛИДАЦИЯ ===

        // MaxDistance
        if (maxDistanceKm.HasValue && (maxDistanceKm < 1 || maxDistanceKm > 500))
            return Result.Failure<ActivityPreferences>(ActivityPreferencesErrors.InvalidMaxDistance);

        if (maxDistanceKm.HasValue && maxDistanceKm < 1)
            return Result.Failure<ActivityPreferences>(ActivityPreferencesErrors.MaxDistanceTooSmall(1));

        if (maxDistanceKm.HasValue && maxDistanceKm > 500)
            return Result.Failure<ActivityPreferences>(ActivityPreferencesErrors.MaxDistanceTooLarge(500));

        // MinParticipants
        if (minParticipants.HasValue && minParticipants < 1)
            return Result.Failure<ActivityPreferences>(ActivityPreferencesErrors.InvalidMinParticipants);

        // MaxParticipants
        if (maxParticipants.HasValue && maxParticipants < 1)
            return Result.Failure<ActivityPreferences>(ActivityPreferencesErrors.InvalidMaxParticipants);

        // MinParticipants vs MaxParticipants
        if (minParticipants.HasValue && maxParticipants.HasValue &&
            minParticipants > maxParticipants)
            return Result.Failure<ActivityPreferences>(ActivityPreferencesErrors.MinParticipantsGreaterThanMax);

        // PreferredActivities - проверка на дубликаты
        if (preferredActivities != null)
        {
            var distinctCount = preferredActivities.Distinct().Count();
            if (preferredActivities.Count() != distinctCount)
                return Result.Failure<ActivityPreferences>(ActivityPreferencesErrors.InvalidActivityType);

            if (preferredActivities.Count() > 20)
                return Result.Failure<ActivityPreferences>(ActivityPreferencesErrors.TooManyPreferredActivities(20));

            foreach (var activity in preferredActivities)
            {
                if (!Enum.IsDefined(typeof(ActivityType), activity))
                    return Result.Failure<ActivityPreferences>(ActivityPreferencesErrors.InvalidActivityType);
            }
        }

        // ActivityLevels - проверка
        if (activityLevels != null)
        {
            if (activityLevels.Count() > 5)
                return Result.Failure<ActivityPreferences>(ActivityPreferencesErrors.TooManyActivityLevels(5));

            foreach (var level in activityLevels)
            {
                if (!Enum.IsDefined(typeof(ActivityLevel), level))
                    return Result.Failure<ActivityPreferences>(ActivityPreferencesErrors.InvalidActivityLevel);
            }
        }

        // PreferredDays - проверка
        if (preferredDays != null)
        {
            foreach (var day in preferredDays)
            {
                if (!Enum.IsDefined(typeof(DayOfWeek), day))
                    return Result.Failure<ActivityPreferences>(ActivityPreferencesErrors.InvalidDayOfWeek);
            }
        }

        // PreferredTimeOfDay
        if (preferredTimeOfDay.HasValue && !Enum.IsDefined(typeof(TimeOfDay), preferredTimeOfDay.Value))
            return Result.Failure<ActivityPreferences>(ActivityPreferencesErrors.InvalidTimeOfDay);

        // Создаем предпочтения
        var preferences = new ActivityPreferences(
            preferredActivities,
            activityLevels,
            preferredTimeOfDay,
            preferredDays,
            maxDistanceKm,
            minParticipants,
            maxParticipants);

        return Result<ActivityPreferences>.Success(preferences);
    }

    public bool HasAnyInterests()
    {
        return PreferredActivities.Any();
    }

    public bool IsActivityPreferred(ActivityType activity)
    {
        return PreferredActivities.Contains(activity);
    }

    public bool IsLevelPreferred(ActivityLevel level)
    {
        return ActivityLevels.Contains(level);
    }

    public bool IsDayPreferred(DayOfWeek day)
    {
        return PreferredDays.Length == 0 || PreferredDays.Contains(day);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return string.Join(",", PreferredActivities.OrderBy(a => a));
        yield return string.Join(",", ActivityLevels.OrderBy(a => a));
        yield return PreferredTimeOfDay;
        yield return string.Join(",", PreferredDays.OrderBy(d => d));
        yield return MaxDistanceKm;
        yield return MinParticipants;
        yield return MaxParticipants;
    }
}


public static class ActivityPreferencesErrors
{
    // === ОШИБКИ ВАЛИДАЦИИ ===

    public static Error InvalidMaxDistance =>
        Error.Validation("Users.ActivityPreferences.InvalidMaxDistance", "Maximum distance must be between 1 and 500 km");

    public static Error MaxDistanceTooSmall(int minValue) =>
        Error.Validation("Users.ActivityPreferences.MaxDistanceTooSmall", $"Maximum distance must be at least {minValue} km");

    public static Error MaxDistanceTooLarge(int maxValue) =>
        Error.Validation("Users.ActivityPreferences.MaxDistanceTooLarge", $"Maximum distance cannot exceed {maxValue} km");

    public static Error InvalidMinParticipants =>
        Error.Validation("Users.ActivityPreferences.InvalidMinParticipants", "Minimum participants must be at least 1");

    public static Error MinParticipantsTooSmall(int minValue) =>
        Error.Validation("Users.ActivityPreferences.MinParticipantsTooSmall", $"Minimum participants must be at least {minValue}");

    public static Error InvalidMaxParticipants =>
        Error.Validation("Users.ActivityPreferences.InvalidMaxParticipants", "Maximum participants must be at least 1");

    public static Error MaxParticipantsTooSmall(int minValue) =>
        Error.Validation("Users.ActivityPreferences.MaxParticipantsTooSmall", $"Maximum participants must be at least {minValue}");

    public static Error MinParticipantsGreaterThanMax =>
        Error.Validation("Users.ActivityPreferences.MinParticipantsGreaterThanMax", "Minimum participants cannot exceed maximum participants");

    public static Error InvalidActivityType =>
        Error.Validation("Users.ActivityPreferences.InvalidActivityType", "Invalid activity type specified");

    public static Error InvalidActivityLevel =>
        Error.Validation("Users.ActivityPreferences.InvalidActivityLevel", "Invalid activity level specified");

    public static Error InvalidTimeOfDay =>
        Error.Validation("Users.ActivityPreferences.InvalidTimeOfDay", "Invalid time of day specified");

    public static Error InvalidDayOfWeek =>
        Error.Validation("Users.ActivityPreferences.InvalidDayOfWeek", "Invalid day of week specified");

    public static Error EmptyPreferredActivities =>
        Error.Validation("Users.ActivityPreferences.EmptyPreferredActivities", "Preferred activities cannot be empty");

    public static Error TooManyPreferredActivities(int maxCount) =>
        Error.Validation("Users.ActivityPreferences.TooManyPreferredActivities", $"Maximum {maxCount} preferred activities allowed");

    public static Error TooManyActivityLevels(int maxCount) =>
        Error.Validation("Users.ActivityPreferences.TooManyActivityLevels", $"Maximum {maxCount} activity levels allowed");

    // === ОШИБКИ НЕ НАЙДЕНО ===

    public static Error NotFound(Guid userId) =>
        Error.NotFound("Users.ActivityPreferences.NotFound", $"Activity preferences not found for user '{userId}'");

    public static Error NotFoundForUser(Guid userId) =>
        Error.NotFound("Users.ActivityPreferences.NotFoundForUser", $"No activity preferences found for user '{userId}'");

    // === ОШИБКИ ОПЕРАЦИЙ ===

    public static Error UpdateFailed(string reason) =>
        Error.Failure("Users.ActivityPreferences.UpdateFailed", $"Failed to update activity preferences: {reason}");

    public static Error CreateFailed(string reason) =>
        Error.Failure("Users.ActivityPreferences.CreateFailed", $"Failed to create activity preferences: {reason}");

    public static Error SaveFailed(string reason) =>
        Error.Failure("Users.ActivityPreferences.SaveFailed", $"Failed to save activity preferences: {reason}");

    public static Error InvalidOperation(string reason) =>
        Error.Problem("Users.ActivityPreferences.InvalidOperation", reason);

    // === ОШИБКИ ОГРАНИЧЕНИЙ ===

    public static Error InvalidRange(string field, int min, int max) =>
        Error.Validation("Users.ActivityPreferences.InvalidRange", $"{field} must be between {min} and {max}");

    public static Error MaxDistanceRequired =>
        Error.Validation("Users.ActivityPreferences.MaxDistanceRequired", "Maximum distance is required");

    public static Error MinParticipantsRequired =>
        Error.Validation("Users.ActivityPreferences.MinParticipantsRequired", "Minimum participants is required");

    public static Error MaxParticipantsRequired =>
        Error.Validation("Users.ActivityPreferences.MaxParticipantsRequired", "Maximum participants is required");
}
