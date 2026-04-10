namespace eMeetup.Modules.Events.Application.EventSessions.GetSessions;

public sealed record SessionResponse(
    Guid Id,
    Guid OrganizerId,
    Guid EventId,
    string Title,
    string Description,
    DateTime StartsAtUtc,
    DateTime? EndsAtUtc,
    string Locality,
    string Address,
    double? Latitude,
    double? Longitude);
