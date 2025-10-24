namespace eMeetup.Common.Domain;

public interface IAuditable
{
    void Created(DateTimeOffset dateTime, string? user);

    void Updated(DateTimeOffset dateTime, string? user);
}
