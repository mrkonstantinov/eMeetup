using System.Reflection;
using System.Xml.Linq;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Errors;

namespace eMeetup.Modules.Users.Domain.Users;

public sealed class User : Entity
{
    private readonly List<Role> _roles = [];
    private readonly List<UserPhoto> _photos = [];
    private readonly List<UserInterest> _interests = [];

    private User() { }

    public Guid Id { get; private set; }
    public string IdentityId { get; private set; }
    public string Email { get; private set; }
    public string UserName { get; private set; }
    public DateTime DateOfBirth { get; private set; } // User's date of birth
    public Gender Gender { get; private set; }        
    public string? Bio { get; private set; }
    public string? ProfilePictureUrl { get; set; }
    public Location? Location { get; private set; }

    //var userLocation = new Point(10.0, 20.0) { SRID = 4326 };

    //var nearbyLocations = context.Locations
    //    .Where(l => l.Coordinate.IsWithinDistance(userLocation, 1000)) // within 1000 meters
    //    .ToList();

    public UserStatus? Status { get; set; }
    //public UserSubscription Subscription { get; set; }
    public DateTime? CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? LastActive { get; private set; }



    // Navigation properties
    public IReadOnlyCollection<UserPhoto> Photos => _photos
    .OrderBy(p => p.DisplayOrder)
    .ThenBy(p => p.UploadedAt)
    .ToList();
    public ICollection<UserInterest> Interests => _interests.AsReadOnly();
    public IReadOnlyCollection<Role> Roles => _roles.ToList();

    public Result AssignRole(Role role)
    {
        if (_roles.Any(r => r.Name == role.Name))
        {
            return Result.Success(); // Already assigned
        }

        _roles.Add(role);
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    //public List<string> Subscriptions { get; set; } = new List<string>();  // List of subscription plans
    //public List<string> Subscribers { get; set; } = new List<string>();  // List of users who subscribed to this profile

    // Constructor for creating new users
    private User(
        string email,
        string username,
        DateTime dateOfBirth,
        Gender gender,
        string? bio,
        string identityId,
        Location? location = null)
    {
        Id = Guid.NewGuid();
        Email = email;
        UserName = username;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        Bio = bio;
        IdentityId = identityId;
        Location = location;
        Status = UserStatus.Active;
        CreatedAt = DateTime.UtcNow;
        LastActive = DateTime.UtcNow;
    }

    // Factory method
    public static Result<User> Create(
        string email,
        string username,
        DateTime dateOfBirth,
        Gender gender,
        string? bio,
        string identityId,
        Location? location = null,
        IEnumerable<Tag>? tags = null)
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
            bio?.Trim(),
            identityId,
            location);

        user._roles.Add(Role.Member);

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

        //user.Raise(new UserRegisteredDomainEvent(user.Id));

        return Result.Success(user);
    }


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

    public void Update(Gender gender, DateTime? dateOfBirth, string? bio, List<UserInterest>? interests, Location location)
    {
        // Check if anything actually changed
        if (Gender == gender &&
            DateOfBirth == dateOfBirth &&
            Bio == bio &&
            AreInterestsEqual(interests) &&
            Location?.Equals(location) == true)
        {
            return;
        }

        Gender = gender;
        Location = location;

        // Update interests if provided
        if (interests != null)
        {
            UpdateInterests(interests);
        }
        UpdatedAt = DateTime.UtcNow;
        // Raise(new UserProfileUpdatedDomainEvent(Id, gender, dateOfBirth, bio, interests, location));
    }

    public Result UpdateProfile(string username, string? bio, Gender gender, List<UserInterest>? interests = null)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Result.Failure(UserErrors.InvalidUsername);

        UserName = username.Trim();
        Bio = bio?.Trim();
        Gender = gender;

        // Update interests if provided
        if (interests != null)
        {
            UpdateInterests(interests);
        }
        else
        {
            UpdatedAt = DateTime.UtcNow;
        }

        return Result.Success();
    }


    public Result UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure(UserErrors.InvalidEmail);

        Email = email.Trim().ToLower();
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result UpdateLocation(Location location)
    {
        if (location == null)
            return Result.Failure(UserErrors.InvalidLocation);

        Location = location;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result UpdateLocation(double latitude, double longitude, string city, string country)
    {
        var locationResult = Location.Create(latitude, longitude, city, country);
        if (locationResult.IsFailure)
            return Result.Failure(locationResult.Error);

        Location = locationResult.Value;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public void ClearLocation()
    {
        Location = null;
        UpdatedAt = DateTime.UtcNow;
    }

    // Photo management methods
    public Result AddPhoto(string url, bool isPrimary = false)
    {
        if (_photos.Count >= 10)
            return Result.Failure(UserErrors.TooManyPhotos(10));

        var displayOrder = _photos.Any() ? _photos.Max(p => p.DisplayOrder) + 1 : 0;

        var photoResult = UserPhoto.Create(Id, url, displayOrder, isPrimary);
        if (photoResult.IsFailure)
            return Result.Failure(photoResult.Error);

        _photos.Add(photoResult.Value);

        if (isPrimary)
        {
            SetPrimaryPhoto(photoResult.Value);
        }

        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result AddPhotos(IEnumerable<string> urls)
    {
        var urlsList = urls.ToList();

        if (_photos.Count + urlsList.Count > 10)
            return Result.Failure(UserErrors.TooManyPhotos(10));

        var currentMaxOrder = _photos.Any() ? _photos.Max(p => p.DisplayOrder) + 1 : 0;

        foreach (var url in urlsList)
        {
            var photoResult = UserPhoto.Create(Id, url, currentMaxOrder);
            if (photoResult.IsFailure)
                return Result.Failure(photoResult.Error);

            _photos.Add(photoResult.Value);
            currentMaxOrder++;
        }

        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result SetPrimaryPhoto(UserPhoto primaryPhoto)
    {
        if (primaryPhoto.UserId != Id)
            return Result.Failure(UserErrors.PhotoNotOwnedByUser);

        if (!_photos.Contains(primaryPhoto))
            return Result.Failure(UserErrors.PhotoNotFound(primaryPhoto.Id));

        foreach (var photo in _photos.Where(p => p.Id != primaryPhoto.Id))
        {
            photo.SetAsSecondary();
        }

        primaryPhoto.SetAsPrimary();

        // Update profile picture URL
        ProfilePictureUrl = primaryPhoto.Url;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result SetPrimaryPhoto(Guid photoId)
    {
        var photo = _photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null)
            return Result.Failure(UserErrors.PhotoNotFound(photoId));

        return SetPrimaryPhoto(photo);
    }

    public Result ReorderPhotos(Dictionary<Guid, int> photoOrders)
    {
        // Validate no duplicate orders
        var duplicateOrders = photoOrders.Values
            .GroupBy(x => x)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateOrders.Any())
            return Result.Failure(UserErrors.DuplicateDisplayOrder);

        // Validate all orders are non-negative
        if (photoOrders.Values.Any(order => order < 0))
            return Result.Failure(UserErrors.InvalidDisplayOrder);

        foreach (var (photoId, newOrder) in photoOrders)
        {
            var photo = _photos.FirstOrDefault(p => p.Id == photoId);
            if (photo == null)
                return Result.Failure(UserErrors.PhotoNotFound(photoId));

            var updateResult = photo.UpdateDisplayOrder(newOrder);
            if (updateResult.IsFailure)
                return updateResult;
        }

        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result RemovePhoto(Guid photoId)
    {
        var photo = _photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null)
            return Result.Failure(UserErrors.PhotoNotFound(photoId));

        _photos.Remove(photo);

        // If we removed the primary photo, set a new primary
        if (photo.IsPrimary && _photos.Any())
        {
            var newPrimary = _photos.OrderBy(p => p.DisplayOrder).First();
            newPrimary.SetAsPrimary();
            ProfilePictureUrl = newPrimary.Url;
        }
        else if (!_photos.Any())
        {
            ProfilePictureUrl = null;
        }

        // Reorder remaining photos
        ReorderRemainingPhotos();
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    private void ReorderRemainingPhotos()
    {
        var orderedPhotos = _photos.OrderBy(p => p.DisplayOrder).ToList();
        for (int i = 0; i < orderedPhotos.Count; i++)
        {
            orderedPhotos[i].UpdateDisplayOrder(i);
        }
    }

    public UserPhoto? GetPrimaryPhoto()
    {
        return _photos.FirstOrDefault(p => p.IsPrimary) ?? _photos.OrderBy(p => p.DisplayOrder).FirstOrDefault();
    }

    public Result<UserPhoto> GetPhoto(Guid photoId)
    {
        var photo = _photos.FirstOrDefault(p => p.Id == photoId);
        return photo != null
            ? Result.Success(photo)
            : Result.Failure<UserPhoto>(UserErrors.PhotoNotFound(photoId));
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
            return Result.Failure(UserErrors.InterestAlreadyAdded);

        var userInterest = UserInterest.Create(Id, tag.Id).Value;

        _interests.Add(userInterest);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result RemoveInterest(Guid tagId)
    {
        var userInterest = _interests.FirstOrDefault(i => i.TagId == tagId);
        if (userInterest == null)
            return Result.Failure(UserErrors.InterestNotFound);

        _interests.Remove(userInterest);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result RemoveInterest(string slug)
    {
        var userInterest = _interests.FirstOrDefault(i =>
            i.Tag.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

        if (userInterest == null)
            return Result.Failure(UserErrors.InterestNotFound);

        _interests.Remove(userInterest);
        UpdatedAt = DateTime.UtcNow;

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
        UpdatedAt = DateTime.UtcNow;

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
