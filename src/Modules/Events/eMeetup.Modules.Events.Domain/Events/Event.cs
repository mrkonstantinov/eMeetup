using System.Data;
using eMeetup.Common.Domain;
using eMeetup.Modules.Events.Domain.EventInterests;

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
    public Guid CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; }      // Snapshot!
    public string CreatedByUserEmail { get; set; }     // Snapshot!
    public string CreatedByUserDisplayName { get; set; } // Snapshot!

    public string Title { get; private set; }
    public string Description { get; private set; }
    public Location Location { get; private set; }

    public DateTime StartsAtUtc { get; private set; }
    public DateTime? EndsAtUtc { get; private set; }

    public DateTime? CreatedAt { get; private set; }
    public EventStatus Status { get; private set; }


    // Navigation properties
    public ICollection<EventTag> Tags => _tags.AsReadOnly();


    public static Result<Event> Create(
        string title,
        string description,
        Location location,
        DateTime startsAtUtc,
        DateTime? endsAtUtc)
    {
        if (endsAtUtc.HasValue && endsAtUtc < startsAtUtc)
        {
            return Result.Failure<Event>(EventErrors.EndDatePrecedesStartDate);
        }

        var @event = new Event
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Location = location,
            StartsAtUtc = startsAtUtc,
            EndsAtUtc = endsAtUtc,
            Status = EventStatus.Draft
        };

        @event.Raise(new EventCreatedDomainEvent(@event.Id));

        return @event;
    }

    public Result Publish()
    {
        if (Status != EventStatus.Draft)
        {
            return Result.Failure(EventErrors.NotDraft);
        }

        Status = EventStatus.Published;

        Raise(new EventPublishedDomainEvent(Id));

        return Result.Success();
    }

    public void Reschedule(DateTime startsAtUtc, DateTime? endsAtUtc)
    {
        if (StartsAtUtc == startsAtUtc && EndsAtUtc == endsAtUtc)
        {
            return;
        }

        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;

        Raise(new EventRescheduledDomainEvent(Id, StartsAtUtc, EndsAtUtc));
    }

    public Result Cancel(DateTime utcNow)
    {
        if (Status == EventStatus.Canceled)
        {
            return Result.Failure(EventErrors.AlreadyCanceled);
        }

        if (StartsAtUtc < utcNow)
        {
            return Result.Failure(EventErrors.AlreadyStarted);
        }

        Status = EventStatus.Canceled;

        Raise(new EventCanceledDomainEvent(Id));

        return Result.Success();
    }
}
