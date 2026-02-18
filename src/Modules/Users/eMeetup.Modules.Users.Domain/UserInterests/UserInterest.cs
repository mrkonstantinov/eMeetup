using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Tags;
using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Domain.UserInterests;

public class UserInterest
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TagId { get; private set; }  
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private UserInterest(Guid userId, Guid tagId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TagId = tagId;
        CreatedAt = DateTime.UtcNow;
    }

    // Factory method
    public static Result<UserInterest> Create(Guid userId, Guid tagId)
    {
        // Validation

        var userInterest = new UserInterest(userId, tagId);
        return Result.Success(userInterest);
    }


    // Navigation properties
    public virtual User User { get; private set; } = null!;
    public virtual Tag Tag { get; private set; } = null!;
}
