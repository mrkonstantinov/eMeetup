namespace eMeetup.Modules.Events.Application.Events.GetEvents;

public sealed record EventResponse(
    Guid Id, 
    Guid OrganizerId, 
    string OrganizerName, 
    string Title, 
    string? Description, 
    string? Url,  
    bool IsArchived,
    string Tags);
