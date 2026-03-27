namespace eMeetup.Modules.Events.Application.Abstractions.Authentication;

public interface IOrganizerContext
{
    Guid OrganizerId { get; }
    string OrganizerName { get; }
}
