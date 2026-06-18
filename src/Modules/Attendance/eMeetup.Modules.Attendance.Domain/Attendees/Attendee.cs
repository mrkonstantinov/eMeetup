using eMeetup.Common.Domain;

namespace eMeetup.Modules.Attendance.Domain.Attendees;

public sealed class Attendee : Entity
{
    private Attendee()
    {
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; }

    public string UserName { get; private set; }

    public DateTime DateOfBirth { get; private set; }

    public Gender Gender { get; private set; }

    public DateTime SyncedAt { get; private set; }

    public static Attendee Create(Guid id, string email, string userName, DateTime dateOfBirth, Gender gender, DateTime syncedAt)
    {
        return new Attendee
        {
            Id = id,
            Email = email,
            UserName = userName,
            DateOfBirth = dateOfBirth,
            Gender = gender, 
            SyncedAt = syncedAt
        };
    }

    public void Update(string userName, DateTime syncedAt)
    {
        UserName = userName;
        SyncedAt = syncedAt;
    }

    //public Result CheckIn(Ticket ticket)
    //{
    //    if (Id != ticket.AttendeeId)
    //    {
    //        Raise(new InvalidCheckInAttemptedDomainEvent(Id, ticket.EventId, ticket.Id, ticket.Code));

    //        return Result.Failure(TicketErrors.InvalidCheckIn);
    //    }

    //    if (ticket.UsedAtUtc.HasValue)
    //    {
    //        Raise(new DuplicateCheckInAttemptedDomainEvent(Id, ticket.EventId, ticket.Id, ticket.Code));

    //        return Result.Failure(TicketErrors.DuplicateCheckIn);
    //    }

    //    ticket.MarkAsUsed();

    //    Raise(new AttendeeCheckedInDomainEvent(Id, ticket.EventId));

    //    return Result.Success();
    //}
}


public enum Gender
{
    Male = 1,
    Female = 2,
    Other = 3,
    PreferNotToSay = 4
}
