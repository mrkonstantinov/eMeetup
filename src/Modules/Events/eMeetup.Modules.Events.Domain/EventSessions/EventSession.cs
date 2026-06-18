using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using eMeetup.Common.Domain;
using eMeetup.Modules.Events.Domain.Events;
using eMeetup.Modules.Events.Domain.MateTypes;

namespace eMeetup.Modules.Events.Domain.EventSessions;

public class EventSession : Entity
{
    private EventSession()
    {
        
    }
    public Guid Id { get; set; } 
    public Guid EventId { get; set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateTime StartsAtUtc { get; set; }
    public DateTime? EndsAtUtc { get; set; }
    public string Locality { get; set; }
    public string Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public EventSessionStatus Status { get; private set; }
    public DateTime? CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? CanceledAt { get; private set; }

    // Navigation properties
    public virtual Event Event { get; set; }

    public static Result<EventSession> Create(
        Guid eventId,
        string title,
        string? description,
        DateTime startsAtUtc,
        DateTime? endsAtUtc,
        string locality,
        string address,
        double? latitude = null,
        double? longitude = null)
    {
        // Validation
        if (eventId == Guid.Empty)
            return Result.Failure<EventSession>(EventSessionErrors.InvalidEventId);

        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<EventSession>(EventSessionErrors.InvalidTitle);

        if (title.Length > 200)
            return Result.Failure<EventSession>(EventSessionErrors.TitleTooLong);

        if (string.IsNullOrWhiteSpace(locality))
            return Result.Failure<EventSession>(EventSessionErrors.InvalidLocality);

        if (string.IsNullOrWhiteSpace(address))
            return Result.Failure<EventSession>(EventSessionErrors.InvalidAddress);

        // Validate dates
        var dateValidation = ValidateDates(startsAtUtc, endsAtUtc);
        if (dateValidation.IsFailure)
            return Result.Failure<EventSession>(dateValidation.Error);

        var eventSession = new EventSession
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Title = title.Trim(),
            Description = description?.Trim(),
            StartsAtUtc = startsAtUtc,
            EndsAtUtc = endsAtUtc,
            Locality = locality.Trim(),
            Address = address.Trim(),
            Latitude = latitude,
            Longitude = longitude,
            Status = EventSessionStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        eventSession.Raise(new SessionCreatedDomainEvent(eventSession.Id, eventId));

        return Result.Success(eventSession);
    }

    // Session management methods
    public Result UpdateDetails(
        string title,
        string? description,
        DateTime startsAtUtc,
        DateTime? endsAtUtc,
        string locality,
        string address,
        double? latitude = null,
        double? longitude = null)
    {
        if (Status != EventSessionStatus.Draft)
            return Result.Failure(EventSessionErrors.CannotModifyPublishedSession);

        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(EventSessionErrors.InvalidTitle);

        if (title.Length > 200)
            return Result.Failure(EventSessionErrors.TitleTooLong);

        if (string.IsNullOrWhiteSpace(locality))
            return Result.Failure(EventSessionErrors.InvalidLocality);

        if (string.IsNullOrWhiteSpace(address))
            return Result.Failure(EventSessionErrors.InvalidAddress);

        var dateValidation = ValidateDates(startsAtUtc, endsAtUtc);
        if (dateValidation.IsFailure)
            return Result.Failure(dateValidation.Error);

        Title = title.Trim();
        Description = description?.Trim();
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        Locality = locality.Trim();
        Address = address.Trim();
        Latitude = latitude;
        Longitude = longitude;
        UpdatedAt = DateTime.UtcNow;

        Raise(new SessionUpdatedDomainEvent(Id));

        return Result.Success();
    }

    public Result Publish()
    {
        if (Status != EventSessionStatus.Draft)
            return Result.Failure(EventSessionErrors.InvalidStatusTransition);

        if (StartsAtUtc <= DateTime.UtcNow)
            return Result.Failure(EventSessionErrors.CannotPublishPastEvent);

        Status = EventSessionStatus.Published;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        Raise(new SessionPublishedDomainEvent(Id));

        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == EventSessionStatus.Completed)
            return Result.Failure(EventSessionErrors.CannotCancelCompletedSession);

        if (Status == EventSessionStatus.Canceled)
            return Result.Success();

        var previousStatus = Status;
        Status = EventSessionStatus.Canceled;
        CanceledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        Raise(new SessionCanceledDomainEvent(Id, previousStatus));

        return Result.Success();
    }

    public Result Complete()
    {
        if (Status != EventSessionStatus.Published)
            return Result.Failure(EventSessionErrors.CannotCompleteUnpublishedSession);

        if (StartsAtUtc > DateTime.UtcNow)
            return Result.Failure(EventSessionErrors.CannotCompleteFutureEvent);

        Status = EventSessionStatus.Completed;
        UpdatedAt = DateTime.UtcNow;

        Raise(new SessionCompletedDomainEvent(Id));

        return Result.Success();
    }

    // Helper methods
    private static Result ValidateDates(DateTime startsAtUtc, DateTime? endsAtUtc)
    {
        if (startsAtUtc < DateTime.UtcNow)
            return Result.Failure(EventSessionErrors.StartDateInPast);

        if (endsAtUtc.HasValue && endsAtUtc.Value <= startsAtUtc)
            return Result.Failure(EventSessionErrors.EndDatePrecedesStartDate);

        return Result.Success();
    }

    // Override for equality
    public override bool Equals(object? obj)
    {
        if (obj is not EventSession other)
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Get all quotas that match user
    /// </summary>
    //public List<Quota> FindMatchingQuotas(UserProfile user)
    //{
    //    return Quotas?
    //        .Where(q => q.MatchesUser(user))
    //        .OrderBy(q => q.Priority)
    //        .ToList() ?? new List<Quota>();
    //}

    /// <summary>
    /// Get quota allocation summary
    /// </summary>
    //public List<QuotaAllocationDto> GetQuotaAllocation(Dictionary<string, int> externalFilledCounts = null)
    //{
    //    if (Quotas == null || !Quotas.Any())
    //        return new List<QuotaAllocationDto>();

    //    return Quotas.Select(q => new QuotaAllocationDto
    //    {
    //        QuotaId = q.Id,
    //        QuotaName = q.Name,
    //        TotalSlots = q.AllocatedSlots,
    //        FilledSlots = externalFilledCounts?.GetValueOrDefault(q.Id, 0) ?? 0,
    //        MinAge = q.MinAge,
    //        MaxAge = q.MaxAge,
    //        Gender = q.Gender,
    //        AllowedCities = q.AllowedCities,
    //        RequiredInterests = q.RequiredInterests,
    //        Priority = q.Priority
    //    }).ToList();
    //}
}
