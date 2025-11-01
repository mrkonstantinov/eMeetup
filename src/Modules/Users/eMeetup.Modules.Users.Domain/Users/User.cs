using System.Reflection;
using System.Xml.Linq;
using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Users;

public sealed class User : Entity
{
    private readonly List<Role> _roles = [];

    private User()
    {
    }

    public Guid Id { get; private set; }
    public string IdentityId { get; private set; }
    public string Email { get; private set; }
    public string UserName { get; private set; }
    public DateTime DateOfBirth { get; private set; } // User's date of birth
    public Gender Gender { get; private set; }        
    public string Bio { get; private set; }
    public string ProfilePictureUrl { get; set; }
    public Location Location { get; private set; }

    //var userLocation = new Point(10.0, 20.0) { SRID = 4326 };

    //var nearbyLocations = context.Locations
    //    .Where(l => l.Coordinate.IsWithinDistance(userLocation, 1000)) // within 1000 meters
    //    .ToList();

    public UserStatus Status { get; set; }
    public UserSubscription Subscription { get; set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime LastActive { get; private set; }

    

    // Navigation properties
    public ICollection<UserPhoto> Photos { get; private set; }
    public ICollection<UserInterest> Interests { get; private set; }
    public IReadOnlyCollection<Role> Roles => _roles.ToList();


    //public List<string> Subscriptions { get; set; } = new List<string>();  // List of subscription plans
    //public List<string> Subscribers { get; set; } = new List<string>();  // List of users who subscribed to this profile

    public static User Create(string email, string userName, DateTime dateOfBirth, Gender gender, string bio, string ProfilePictureUrl, Location location,  string identityId)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = userName,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            Bio = bio, 
            ProfilePictureUrl = ProfilePictureUrl, 
            Location = location,
            IdentityId = identityId
        };

        user._roles.Add(Role.Member);

        //user.Raise(new UserRegisteredDomainEvent(user.Id));

        return user;
    }

    public void Update(Gender gender, DateTime? dateOfBirth, string? bio, string? interests, Location location)
    {
        //if (DateOfBirth == dateOfBirth && Interests == interests)
        {
            return;
        }

        Gender = gender; 

        Location = location;

        //Raise(new UserProfileUpdatedDomainEvent(Id, gender, dateOfBirth, bio, interests, location));
    }

    public void UploadUserImage(string? profilePictureUrl)
    {
        //Raise(new UserImageUploadedDomainEvent(Id, profilePictureUrl));
    }
}
