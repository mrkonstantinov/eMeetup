using eMeetup.Common.Domain;

namespace eMeetup.Modules.Events.Domain.MateTypes;

public static class MateTypeErrors
{
    public static readonly Error InvalidEventSessionId = Error.Problem(
        "MateTypes.InvalidEventSessionId", "Event session ID is required");

    public static readonly Error InvalidTitle = Error.Problem(
        "MateTypes.InvalidName", "Mate type name is required");

    public static readonly Error TitleTooLong = Error.Problem(
        "MateTypes.NameTooLong", "Mate type name cannot exceed 100 characters");

    public static readonly Error InvalidAllocatedSlots = Error.Problem(
        "MateTypes.InvalidAllocatedSlots", "Allocated slots must be greater than 0");

    public static readonly Error InvalidAgeRange = Error.Problem(
        "MateTypes.InvalidAgeRange", "Minimum age cannot be greater than maximum age");
}
