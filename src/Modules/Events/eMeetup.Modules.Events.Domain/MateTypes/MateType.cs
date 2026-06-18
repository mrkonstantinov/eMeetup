using eMeetup.Common.Domain;
using eMeetup.Modules.Events.Domain.Events;
using eMeetup.Modules.Events.Domain.EventSessions;

namespace eMeetup.Modules.Events.Domain.MateTypes;

public sealed class MateType : Entity
{
    private MateType() { }

    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public int AllocatedSlots { get; private set; }
    public decimal? Budget { get; private set; }
    public int? MinAge { get; private set; }
    public int? MaxAge { get; private set; }
    public Gender? Gender { get; private set; }
    public Gender? PreferredGender { get; private set; }
    public string? PreferredAgeRange { get; private set; }
    public int Priority { get; private set; }
    public DateTime? CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }


    // Computed properties (filled by external module)
    public int FilledSlots { get; internal set; }
    public int AvailableSlots => AllocatedSlots - FilledSlots;
    public bool IsFull => AvailableSlots <= 0;

    // Factory method
    public static Result<MateType> Create(
        Guid sessionId,
        string title,
        string? description,
        int allocatedSlots,
        decimal? budget,
        int? minAge,
        int? maxAge,
        Gender? gender,        
        Gender? preferredGender,
        string? preferredAgeRange,
        int priority = 0)
    {
        // Validation
        if (sessionId == Guid.Empty)
            return Result.Failure<MateType>(MateTypeErrors.InvalidSessionId);

        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<MateType>(MateTypeErrors.InvalidTitle);

        if (title.Length > 100)
            return Result.Failure<MateType>(MateTypeErrors.TitleTooLong);

        if (allocatedSlots <= 0)
            return Result.Failure<MateType>(MateTypeErrors.InvalidAllocatedSlots);

        // Validate age range
        if (minAge.HasValue && maxAge.HasValue && minAge > maxAge)
            return Result.Failure<MateType>(MateTypeErrors.InvalidAgeRange);

        var mateType = new MateType
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Title = title.Trim(),
            Description = description?.Trim(),
            AllocatedSlots = allocatedSlots,
            Budget = budget,
            MinAge = minAge,
            MaxAge = maxAge,
            Gender = gender,
            PreferredGender = preferredGender,
            PreferredAgeRange = preferredAgeRange,
            Priority = priority,
            CreatedAt = DateTime.UtcNow
        };

        return Result.Success(mateType);
    }

    // Update method
    public Result Update(
        string name,
        string? description,
        int allocatedSlots,
        decimal? budget,
        int? minAge,
        int? maxAge,
        Gender? gender,
        List<string>? requiredInterests,
        List<string>? allowedCities,
        Gender? preferredGender,
        string? preferredAgeRange,
        int priority)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(MateTypeErrors.InvalidTitle);

        if (name.Length > 100)
            return Result.Failure(MateTypeErrors.TitleTooLong);

        if (allocatedSlots <= 0)
            return Result.Failure(MateTypeErrors.InvalidAllocatedSlots);

        if (minAge.HasValue && maxAge.HasValue && minAge > maxAge)
            return Result.Failure(MateTypeErrors.InvalidAgeRange);

        Title = name.Trim();
        Description = description?.Trim();
        AllocatedSlots = allocatedSlots;
        Budget = budget;
        MinAge = minAge;
        MaxAge = maxAge;
        Gender = gender;
        PreferredGender = preferredGender;
        PreferredAgeRange = preferredAgeRange;
        Priority = priority;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }


    public string GetRequirementsText()
    {
        var requirements = new List<string>();

        if (MinAge.HasValue && MaxAge.HasValue)
            requirements.Add($"Age {MinAge}-{MaxAge}");
        else if (MinAge.HasValue)
            requirements.Add($"Min age {MinAge}");
        else if (MaxAge.HasValue)
            requirements.Add($"Max age {MaxAge}");

        if (Gender.HasValue)
            requirements.Add($"Gender: {Gender}");

        return requirements.Any() ? string.Join(", ", requirements) : "No restrictions";
    }

    public bool HasAvailableSlots(int currentFilledCount) => currentFilledCount < AllocatedSlots;

    public override bool Equals(object? obj)
    {
        if (obj is not MateType other)
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}
