using System.Data;
using eMeetup.Common.Domain;
using eMeetup.Modules.Events.Domain.EventTags;

namespace eMeetup.Modules.Events.Domain.Events;

public sealed class Event : Entity
{
    // Private fields
    private readonly List<EventTag> _tags = new();
    private Event()
    {
    }

    public Guid Id { get; private set; }
    // User information - COMPLETE SNAPSHOT at creation time
    public Guid OrganizerId { get; set; }
    public string OrganizerName { get; set; }      // Snapshot!

    public string Title { get; private set; }
    public string? Description { get; private set; }
    public string? Url { get; private set; }
    public DateTime? CreatedAt { get; private set; }
    //public EventStatus Status { get; private set; }
    public bool IsArchived { get; private set; }


    // Navigation properties
    public ICollection<EventTag> Tags => _tags.AsReadOnly();


    public static Result<Event> Create(
        Guid organizerId,
        string organizerName,
        string title,
        string? description,
        string? url,        
        string? tags)
    {
        //if (endsAtUtc.HasValue && endsAtUtc < startsAtUtc)
        //{
        //    return Result.Failure<Event>(EventErrors.EndDatePrecedesStartDate);
        //}

        var @event = new Event
        {
            Id = Guid.NewGuid(),
            OrganizerId = organizerId,
            OrganizerName = organizerName,
            Title = title,
            Description = description,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        @event.Raise(new EventCreatedDomainEvent(@event.Id));

        return @event;
    }

    //public Result Publish()
    //{
    //    if (Status != EventStatus.Draft)
    //    {
    //        return Result.Failure(EventErrors.NotDraft);
    //    }

    //    Status = EventStatus.Published;

    //    Raise(new EventPublishedDomainEvent(Id));

    //    return Result.Success();
    //}

    //public void Reschedule(DateTime startsAtUtc, DateTime? endsAtUtc)
    //{
    //    if (StartsAtUtc == startsAtUtc && EndsAtUtc == endsAtUtc)
    //    {
    //        return;
    //    }

    //    StartsAtUtc = startsAtUtc;
    //    EndsAtUtc = endsAtUtc;

    //    Raise(new EventRescheduledDomainEvent(Id, StartsAtUtc, EndsAtUtc));
    //}

    //public Result Cancel(DateTime utcNow)
    //{
    //    if (Status == EventStatus.Canceled)
    //    {
    //        return Result.Failure(EventErrors.AlreadyCanceled);
    //    }

    //    //if (StartsAtUtc < utcNow)
    //    //{
    //    //    return Result.Failure(EventErrors.AlreadyStarted);
    //    //}

    //    Status = EventStatus.Canceled;

    //    Raise(new EventCanceledDomainEvent(Id));

    //    return Result.Success();
    //}
    public void Archive()
    {
        IsArchived = true;

        Raise(new EventArchivedDomainEvent(Id));
    }
}
