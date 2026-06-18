namespace eMeetup.Modules.Events.Application.Events.GetEvents;

public sealed record EventResponse(
    Guid Id, 
    Guid CreatorId, 
    string Title, 
    string? Description, 
    string? Url,  
    bool IsArchived,
    string Tags);
