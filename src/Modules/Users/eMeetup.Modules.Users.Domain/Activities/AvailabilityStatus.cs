using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Activities;

public class AvailabilityStatus : ValueObject
{
    public AvailabilityType Type { get; }
    public DateTime? AvailableFrom { get; }
    public DateTime? AvailableUntil { get; }
    public string? StatusMessage { get; }

    private AvailabilityStatus() { }

    private AvailabilityStatus(
        AvailabilityType type,
        DateTime? availableFrom = null,
        DateTime? availableUntil = null,
        string? statusMessage = null)
    {
        Type = type;
        AvailableFrom = availableFrom;
        AvailableUntil = availableUntil;
        StatusMessage = statusMessage?.Trim();
    }

    public static AvailabilityStatus Available()
    {
        return new AvailabilityStatus(AvailabilityType.Available);
    }

    public static AvailabilityStatus Busy()
    {
        return new AvailabilityStatus(AvailabilityType.Busy);
    }

    public static AvailabilityStatus Away()
    {
        return new AvailabilityStatus(AvailabilityType.Away);
    }

    public static Result<AvailabilityStatus> Unavailable(
        DateTime? until = null,
        string? message = null)
    {
        if (until.HasValue && until.Value < DateTime.UtcNow)
            return Result.Failure<AvailabilityStatus>(Error.Failure("Users.AvailabilityStatus.InvalidDate", "Until date must be in future"));

        return Result<AvailabilityStatus>.Success(
            new AvailabilityStatus(AvailabilityType.Unavailable, null, until, message));
    }

    public bool IsAvailable()
    {
        return Type == AvailabilityType.Available;
    }

    public bool CanParticipate()
    {
        if (Type == AvailabilityType.Available)
            return true;

        if (Type == AvailabilityType.Unavailable && AvailableUntil.HasValue)
            return AvailableUntil.Value < DateTime.UtcNow;

        return false;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return AvailableFrom;
        yield return AvailableUntil;
        yield return StatusMessage;
    }
}

public enum AvailabilityType
{
    Available,
    Busy,
    Away,
    Unavailable
}
