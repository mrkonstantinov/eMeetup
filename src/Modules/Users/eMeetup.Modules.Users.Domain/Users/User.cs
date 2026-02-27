using System.Reflection;
using System.Xml.Linq;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Errors;
using eMeetup.Modules.Users.Domain.Tags;
using eMeetup.Modules.Users.Domain.UserInterests;

namespace eMeetup.Modules.Users.Domain.Users;

public sealed class User : Entity
{
    // Private fields
    private readonly List<Role> _roles = new();
    private readonly List<UserPhoto> _photos = new();
    private readonly List<UserInterest> _interests = new();

    private User() { }

    public Guid Id { get; private set; }
    public string IdentityId { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string UserName { get; private set; } = null!;
    public DateTime DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    public string? Bio { get; private set; }
    public string? ProfilePictureUrl { get; private set; }
    public Location? Location { get; private set; }
    public UserStatus? Status { get; private set; }
    public DateTime? CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? LastActive { get; private set; }
    // Concurrency token - using uint for xmin

    // Navigation properties
    public IReadOnlyCollection<UserPhoto> Photos => _photos
        .OrderBy(p => p.DisplayOrder)
        .ThenBy(p => p.UploadedAt)
        .ToList()
        .AsReadOnly();
    public ICollection<UserInterest> Interests => _interests.AsReadOnly();
    public IReadOnlyCollection<Role> Roles => _roles.ToList();

    public Result AssignRole(Role role)
    {
        if (_roles.Any(r => r.Name == role.Name))
        {
            return Result.Success(); // Already assigned
        }

        _roles.Add(role);
        return Result.Success();
    }

    //public List<string> Subscriptions { get; set; } = new List<string>();  // List of subscription plans
    //public List<string> Subscribers { get; set; } = new List<string>();  // List of users who subscribed to this profile

    // Constructor for basic user registration (required fields only)
    private User(
        string email,
        string username,
        DateTime dateOfBirth,
        Gender gender,
        string identityId)
    {
        Id = Guid.NewGuid();
        Email = email;
        UserName = username;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        IdentityId = identityId;
        Status = UserStatus.Active;
        CreatedAt = DateTime.UtcNow;
        LastActive = DateTime.UtcNow;

        // Assign default role
        _roles.Add(Role.Member);
    }


    // Factory method for registration (basic information only)
    public static Result<User> Create(
        string email,
        string username,
        DateTime dateOfBirth,
        Gender gender,
        string identityId)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<User>(UserErrors.InvalidEmail);

        if (string.IsNullOrWhiteSpace(username))
            return Result.Failure<User>(UserErrors.InvalidUsername);

        if (string.IsNullOrWhiteSpace(identityId))
            return Result.Failure<User>(UserErrors.InvalidIdentityId);

        if (!IsValidAge(dateOfBirth))
            return Result.Failure<User>(UserErrors.InvalidDateOfBirth);

        var user = new User(
            email.Trim().ToLower(),
            username.Trim(),
            dateOfBirth,
            gender,
            identityId);

        //user.Raise(new UserRegisteredDomainEvent(user.Id));

        return Result.Success(user);
    }

    public static Result<User> CreateWithProfile(
        string email,
        string username,
        DateTime dateOfBirth,
        Gender gender,
        string identityId,
        string? bio = null,
        Location? location = null,
        IEnumerable<Tag>? tags = null)
    {
        var createResult = Create(email, username, dateOfBirth, gender, identityId);
        if (createResult.IsFailure)
            return createResult;

        var user = createResult.Value;

        // Set optional fields if provided
        if (!string.IsNullOrWhiteSpace(bio))
        {
            user.UpdateBio(bio);
        }

        if (location != null)
        {
            user.UpdateLocation(location);
        }

        if (tags != null && tags.Any())
        {
            foreach (var tag in tags)
            {
                if (tag != null && tag.IsActive)
                {
                    user.AddInterest(tag);
                }
            }
        }

        return Result.Success(user);
    }

    // Update basic information (for UpdateUserCommand)
    //public Result UpdateBasicInfo(
    //    string email,
    //    string username,
    //    DateTime dateOfBirth,
    //    Gender gender)
    //{
    //    if (string.IsNullOrWhiteSpace(email))
    //        return Result.Failure(UserErrors.InvalidEmail);

    //    if (string.IsNullOrWhiteSpace(username))
    //        return Result.Failure(UserErrors.InvalidUsername);

    //    if (!IsValidAge(dateOfBirth))
    //        return Result.Failure(UserErrors.InvalidDateOfBirth);

    //    Email = email.Trim().ToLower();
    //    UserName = username.Trim();
    //    DateOfBirth = dateOfBirth;
    //    Gender = gender;
    //    UpdatedAt = DateTime.UtcNow;

    //    return Result.Success();
    //}

    private bool AreInterestsEqual(List<UserInterest>? otherInterests)
    {
        if (_interests.Count != otherInterests?.Count)
            return false;

        return _interests.All(i => otherInterests.Any(oi => oi.Id == i.Id)) &&
               otherInterests.All(oi => _interests.Any(i => i.Id == oi.Id));
    }

    private void UpdateInterests(List<UserInterest> newInterests)
    {
        // Clear existing interests
        _interests.Clear();

        // Add new interests
        _interests.AddRange(newInterests);

        UpdatedAt = DateTime.UtcNow;
    }

    // Individual update methods for UpdateUserCommandHandler
    public Result UpdateBio(string? bio)
    {
        Bio = bio?.Trim();
        return Result.Success();
    }

    public Result UpdateProfilePictureUrl(string? profilePictureUrl)
    {
        ProfilePictureUrl = profilePictureUrl?.Trim();
        return Result.Success();
    }

    public Result UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure(UserErrors.InvalidEmail);

        Email = email.Trim().ToLower();

        return Result.Success();
    }

    public Result UpdateLocation(Location location)
    {
        if (location == null)
            return Result.Failure(UserErrors.InvalidLocation);

        Location = location;

        return Result.Success();
    }

    public Result UpdateLocation(double latitude, double longitude, string city, string street)
    {
        var locationResult = Location.Create(latitude, longitude, city, street);
        if (locationResult.IsFailure)
            return Result.Failure(locationResult.Error);

        Location = locationResult.Value;

        return Result.Success();
    }

    public void ClearLocation()
    {
        Location = null;
    }

    // Role management methods
    //public Result AssignRole(Role role)
    //{
    //    if (_roles.Any(r => r.Id == role.Id))
    //        return Result.Failure(UserErrors.RoleAlreadyAssigned);

    //    _roles.Add(role);
    //    UpdatedAt = DateTime.UtcNow;

    //    return Result.Success();
    //}

    //public Result RemoveRole(Role role)
    //{
    //    var existingRole = _roles.FirstOrDefault(r => r.Id == role.Id);
    //    if (existingRole == null)
    //        return Result.Failure(UserErrors.RoleNotAssigned);

    //    _roles.Remove(existingRole);
    //    UpdatedAt = DateTime.UtcNow;

    //    return Result.Success();
    //}

    public bool HasRole(string roleName)
    {
        return _roles.Any(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }

    // Interest management methods
    public Result AddInterest(Tag tag)
    {
        if (tag == null)
            return Result.Failure(TagErrors.NotFound);

        if (_interests.Any(ui => ui.TagId == tag.Id))
            return Result.Failure(UserInterestErrors.InterestAlreadyAdded);

        var userInterest = UserInterest.Create(Id, tag.Id).Value;

        _interests.Add(userInterest);

        return Result.Success();
    }

    public Result RemoveInterest(Guid tagId)
    {
        var userInterest = _interests.FirstOrDefault(i => i.TagId == tagId);
        if (userInterest == null)
            return Result.Failure(UserInterestErrors.InterestNotFound);

        _interests.Remove(userInterest);

        return Result.Success();
    }

    public Result RemoveInterest(string slug)
    {
        var userInterest = _interests.FirstOrDefault(i =>
            i.Tag.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

        if (userInterest == null)
            return Result.Failure(UserInterestErrors.InterestNotFound);

        _interests.Remove(userInterest);

        return Result.Success();
    }

    public bool HasInterest(Guid tagId) => _interests.Any(i => i.TagId == tagId);
    public bool HasInterest(string slug) => _interests.Any(i =>
        i.Tag.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

    // Status management methods
    public Result UpdateStatus(UserStatus status)
    {
        // Validate status transition
        if (!IsValidStatusTransition((UserStatus)Status, status))
            return Result.Failure(UserErrors.InvalidStatusTransition);

        Status = status;

        if (status == UserStatus.Active)
        {
            LastActive = DateTime.UtcNow;
        }

        return Result.Success();
    }

    private static bool IsValidStatusTransition(UserStatus fromStatus, UserStatus toStatus)
    {
        return (fromStatus, toStatus) switch
        {
            (UserStatus.Active, UserStatus.Inactive) => true,
            (UserStatus.Active, UserStatus.Suspended) => true,
            (UserStatus.Inactive, UserStatus.Active) => true,
            (UserStatus.Suspended, UserStatus.Active) => true,
            (_, UserStatus.Deleted) => true,
            _ => false
        };
    }

    public void MarkAsActive()
    {
        LastActive = DateTime.UtcNow;
    }

    public Result Delete()
    {
        return UpdateStatus(UserStatus.Deleted);
    }

    // Business logic methods
    public bool IsNearby(Location otherLocation, double radiusKm = 50)
    {
        return Location?.IsWithinRadius(otherLocation, radiusKm) ?? false;
    }

    public int GetAge()
    {
        var age = DateTime.Today.Year - DateOfBirth.Year;
        if (DateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
        return age;
    }

    public bool IsAdult() => GetAge() >= 18;

    private static bool IsValidAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;

        // Adjust age if the birthday hasn't occurred this year yet
        if (dateOfBirth.Date > today.AddYears(-age))
        {
            age--;
        }

        return age >= 18;
    }

    public bool CanContact(User otherUser)
    {
        // Business rules for user contact
        return Status == UserStatus.Active &&
               otherUser.Status == UserStatus.Active &&
               !HasRole("Blocked") &&
               !otherUser.HasRole("Blocked");
    }
}
