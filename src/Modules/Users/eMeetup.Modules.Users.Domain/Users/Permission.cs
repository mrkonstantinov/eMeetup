namespace eMeetup.Modules.Users.Domain.Users;
public sealed class Permission
{
    public static readonly Permission GetUser = new("users:read");
    public static readonly Permission ModifyUser = new("users:update");
    public static readonly Permission GetTags = new("tags:read");
    public static readonly Permission GetEvents = new("events:read");
    public static readonly Permission SearchEvents = new("events:search");
    public static readonly Permission ModifyEvents = new("events:update");
    public static readonly Permission GetEventStatistics = new("event-statistics:read");

    public Permission(string code)
    {
        Code = code;
    }

    public string Code { get; }
}
