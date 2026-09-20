using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Activities;
using eMeetup.Modules.Users.Domain.Helpers;
using eMeetup.Modules.Users.Domain.Photos;
using eMeetup.Modules.Users.Domain.Tags;

namespace eMeetup.Modules.Users.Domain.Users;

public sealed class User : Entity
{
// === PRIVATE FIELDS ===
    private readonly List<UserPhoto> _photos = new();
    private readonly List<UserTag> _userTags = new();
    private readonly List<UserFavorite> _favorites = new();
    private readonly List<UserFavorite> _favoritedBy = new();
    private readonly List<UserSubscription> _subscriptions = new();
    private readonly List<UserSubscription> _subscribers = new();
    private readonly List<Role> _roles = new();
    //private readonly List<UserFavorite> _favorites = new();


    // === PUBLIC PROPERTIES ===
    public Guid Id { get; private set; }
    public Guid IdentityId { get; private set; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public UserProfile Profile { get; private set; }

    public UserStatus Status { get; private set; }
    public bool ProfileCompleted { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? LastActiveAt { get; private set; }
    public DateTime? LastStatusChangeAt { get; private set; }
    public string? StatusReason { get; private set; }

    // === NAVIGATION PROPERTIES ===
    public IReadOnlyCollection<UserPhoto> Photos => _photos.AsReadOnly();
    public string? ProfileImageUrl => _photos.GetPrimaryUrl();
    public IReadOnlyCollection<UserTag> UserTags => _userTags.AsReadOnly();
    // Избранное
    public IReadOnlyCollection<UserFavorite> Favorites => _favorites.AsReadOnly();
    public IReadOnlyCollection<UserFavorite> FavoritedBy => _favoritedBy.AsReadOnly();
    // Подписки
    public IReadOnlyCollection<UserSubscription> Subscriptions => _subscriptions.AsReadOnly();
    public IReadOnlyCollection<UserSubscription> Subscribers => _subscribers.AsReadOnly();
    public IReadOnlyCollection<Role> Roles => _roles.ToList();

    // === PRIVATE CONSTRUCTOR FOR EF ===
    private User() { }

    private User(Guid identityId, string username, string email)
    {
        Id = Guid.NewGuid();
        IdentityId = identityId;
        Username = username;
        Email = email;
        Profile = UserProfile.CreateMinimal(username, email);
        Status = UserStatus.Inactive;
        ProfileCompleted = false;
        CreatedAt = DateTime.UtcNow;
        LastStatusChangeAt = DateTime.UtcNow;
    }

    // === FACTORY METHODS ===

    /// <summary>
    /// Создает пользователя после регистрации в Keycloak
    /// </summary>
    public static Result<User> CreateFromKeycloak(Guid keycloakId, string username, string email)
    {
        if (Guid.Empty == (keycloakId))
            return Result.Failure<User>(RegistrationErrors.InvalidKeycloakId);

        if (string.IsNullOrWhiteSpace(username))
            return Result<User>.Failure<User>(RegistrationErrors.InvalidUsername(username));

        if (string.IsNullOrWhiteSpace(email))
            return Result<User>.Failure<User>(RegistrationErrors.InvalidEmail(email));

        if (!IsValidEmail(email))
            return Result<User>.Failure<User>(RegistrationErrors.InvalidEmail(email));

        if (username.Length < 3)
            return Result<User>.Failure<User>(RegistrationErrors.UsernameTooShort(3));

        if (username.Length > 50)
            return Result<User>.Failure<User>(RegistrationErrors.UsernameTooLong(50));

        var user = new User(keycloakId, username, email);
        return Result<User>.Success(user);
    }


    // === PHOTO MANAGEMENT ===

    /// <summary>
    /// Добавляет фото пользователю
    /// </summary>
    public Result AddPhoto(string url, bool isPrimary = false)
    {
        if (!CanParticipate())
            return Result.Failure(PhotoErrors.CannotUploadForInactiveUser);

        if (string.IsNullOrWhiteSpace(url))
            return Result.Failure(PhotoErrors.EmptyPhoto);

        if (_photos.Count >= 10)
            return Result.Failure(PhotoErrors.TooManyPhotos(10));

        var shouldBePrimary = isPrimary || !_photos.Any();

        var photo = UserPhoto.Create(Id, url, _photos.Count, shouldBePrimary);
        if (photo.IsFailure)
            return Result.Failure(photo.Error);

        if (shouldBePrimary)
        {
            foreach (var p in _photos)
                p.SetAsSecondary();
        }

        _photos.Add(photo.Value);

        if (shouldBePrimary)
            Profile = Profile.UpdateAvatar(url);

        UpdatedAt = DateTime.UtcNow;
        Raise(new UserPhotoAddedEvent(Id, url, isPrimary));

        return Result.Success();
    }


    /// <summary>
    /// Устанавливает основное фото
    /// </summary>
    public Result SetPrimaryPhoto(Guid photoId)
    {
        if (!CanParticipate())
            return Result.Failure(PhotoErrors.CannotUploadForInactiveUser);

        var photo = _photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null)
            return Result.Failure(PhotoErrors.NotFound(photoId));

        foreach (var p in _photos)
            p.SetAsSecondary();

        photo.SetAsPrimary();
        Profile = Profile.UpdateAvatar(photo.Url);

        UpdatedAt = DateTime.UtcNow;
        Raise(new UserPrimaryPhotoChangedEvent(Id, photoId));

        return Result.Success();
    }

    /// <summary>
    /// Удаляет фото
    /// </summary>
    public Result RemovePhoto(Guid photoId)
    {
        if (Status == UserStatus.Deleted)
            return Result.Failure(PhotoErrors.CannotUploadForDeletedUser);

        var photo = _photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null)
            return Result.Failure(PhotoErrors.NotFound(photoId));

        var wasPrimary = photo.IsPrimary;
        _photos.Remove(photo);

        if (wasPrimary)
        {
            var newPrimary = _photos.FirstOrDefault();
            if (newPrimary != null)
            {
                newPrimary.SetAsPrimary();
                Profile = Profile.UpdateAvatar(newPrimary.Url);
            }
            else
            {
                Profile = Profile.UpdateAvatar(null);
            }
        }

        UpdatedAt = DateTime.UtcNow;
        Raise(new UserPhotoRemovedEvent(Id, photoId));

        return Result.Success();
    }

    /// <summary>
    /// Изменяет порядок фото
    /// </summary>
    public Result ReorderPhotos(Dictionary<Guid, int> orderMap)
    {
        if (!CanParticipate())
            return Result.Failure(PhotoErrors.CannotUploadForInactiveUser);

        foreach (var (photoId, newOrder) in orderMap)
        {
            var photo = _photos.FirstOrDefault(p => p.Id == photoId);
            if (photo == null)
                return Result.Failure(PhotoErrors.NotFound(photoId));

            photo.UpdateDisplayOrder(newOrder);
        }

        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    // === TAG MANAGEMENT ===

    /// <summary>
    /// Добавляет тег пользователю
    /// </summary>
    public Result AddTag(UserTag tag)
    {
        if (!CanParticipate())
            return Result.Failure(TagErrors.CannotAddTagToInactiveUser);

        if (tag == null)
            return Result.Failure(TagErrors.InvalidTagName);

        if (!tag.IsActive)
            return Result.Failure(TagErrors.TagNotActive);

        // Проверяем, есть ли уже такой тег у пользователя
        if (_userTags.Any(ut => ut.Id == tag.Id && ut.IsActive))
            return Result.Failure(TagErrors.AlreadyExists(tag.Name));

        if (_userTags.Count(ut => ut.IsActive) >= 20)
            return Result.Failure(TagErrors.TooManyTags(20));

        // Добавляем тег пользователю
        _userTags.Add(tag);
        tag.IncrementUsage();

        UpdateProfileInterests();
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }


    /// <summary>
    /// Удаляет тег у пользователя
    /// </summary>
    public Result RemoveTag(UserTag tag)
    {
        if (tag == null)
            return Result.Failure(TagErrors.InvalidTagName);

        if (Status == UserStatus.Deleted)
            return Result.Failure(TagErrors.CannotRemoveTagFromDeletedUser);

        // Проверяем, есть ли тег у пользователя
        if (!_userTags.Any(ut => ut.Id == tag.Id && ut.IsActive))
            return Result.Failure(TagErrors.TagNotFoundForUser(tag.Id, Id));

        // Удаляем тег
        _userTags.Remove(tag);
        tag.DecrementUsage();

        UpdateProfileInterests();
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Удаляет тег по ID
    /// </summary>
    public Result RemoveTagById(Guid tagId)
    {
        var tag = _userTags.FirstOrDefault(ut => ut.Id == tagId && ut.IsActive);
        if (tag == null)
            return Result.Failure(TagErrors.TagNotFoundForUser(tagId, Id));

        return RemoveTag(tag);
    }

    /// <summary>
    /// Добавляет несколько тегов
    /// </summary>
    public Result AddTags(IEnumerable<UserTag> tags)
    {
        if (tags == null || !tags.Any())
            return Result.Success();

        foreach (var tag in tags)
        {
            var result = AddTag(tag);
            if (result.IsFailure)
                return result;
        }
        return Result.Success();
    }

    /// <summary>
    /// Удаляет несколько тегов
    /// </summary>
    public Result RemoveTags(IEnumerable<UserTag> tags)
    {
        if (tags == null || !tags.Any())
            return Result.Success();

        foreach (var tag in tags)
        {
            var result = RemoveTag(tag);
            if (result.IsFailure)
                return result;
        }
        return Result.Success();
    }

    /// <summary>
    /// Очищает все теги
    /// </summary>
    public Result ClearTags()
    {
        if (Status == UserStatus.Deleted)
            return Result.Failure(TagErrors.CannotRemoveTagFromDeletedUser);

        var activeTags = _userTags.Where(ut => ut.IsActive).ToList();
        foreach (var tag in activeTags)
        {
            tag.DecrementUsage();
        }

        _userTags.Clear();

        UpdateProfileInterests();
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Получает активные теги пользователя
    /// </summary>
    public IReadOnlyList<UserTag> GetActiveTags()
    {
        return _userTags
            .Where(ut => ut.IsActive)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Получает теги по группе
    /// </summary>
    public IReadOnlyList<UserTag> GetTagsByGroup(Guid groupId)
    {
        return GetActiveTags()
            .Where(t => t.TagGroupId == groupId)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Проверяет наличие тега
    /// </summary>
    public bool HasTag(Guid tagId)
    {
        return _userTags.Any(ut => ut.Id == tagId && ut.IsActive);
    }

    /// <summary>
    /// Проверяет наличие тега по имени
    /// </summary>
    public bool HasTag(string tagName)
    {
        return _userTags.Any(ut =>
            ut.IsActive &&
            ut.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Получает количество тегов
    /// </summary>
    public int GetTagsCount()
    {
        return _userTags.Count(ut => ut.IsActive);
    }

    /// <summary>
    /// Получает количество тегов по группе
    /// </summary>
    public int GetTagsCountByGroup(Guid groupId)
    {
        return _userTags.Count(ut =>
            ut.IsActive &&
            ut.TagGroupId == groupId);
    }

    // === FAVORITE MANAGEMENT ===

    /// <summary>
    /// Добавляет пользователя в избранное
    /// </summary>
    public Result AddFavorite(Guid targetUserId)
    {
        if (Id == targetUserId)
            return Result.Failure(FavoriteErrors.CannotAddYourself);

        if (!CanParticipate())
            return Result.Failure(FavoriteErrors.CannotAddInactiveUser);

        if (_favorites.Any(f => f.TargetUserId == targetUserId && f.IsActive))
            return Result.Failure(FavoriteErrors.AlreadyExists(targetUserId));

        if (_favorites.Count(f => f.IsActive) >= 100)
            return Result.Failure(FavoriteErrors.TooManyFavorites(100));

        var favorite = UserFavorite.Create(Id, targetUserId);
        if (favorite.IsFailure)
            return Result.Failure(favorite.Error);

        _favorites.Add(favorite.Value);
        UpdatedAt = DateTime.UtcNow;
        //77AddDomainEvent(new UserFavoriteAddedEvent(Id, targetUserId));

        return Result.Success();
    }

    /// <summary>
    /// Удаляет пользователя из избранного
    /// </summary>
    public Result RemoveFavorite(Guid targetUserId)
    {
        var favorite = _favorites.FirstOrDefault(f => f.TargetUserId == targetUserId && f.IsActive);
        if (favorite == null)
            return Result.Failure(FavoriteErrors.NotFound(Id, targetUserId));

        favorite.Remove();
        UpdatedAt = DateTime.UtcNow;
        //77vAddDomainEvent(new UserFavoriteRemovedEvent(Id, targetUserId));

        return Result.Success();
    }

    /// <summary>
    /// Удаляет из избранного по ID
    /// </summary>
    public Result RemoveFavoriteById(Guid favoriteId)
    {
        var favorite = _favorites.FirstOrDefault(f => f.Id == favoriteId);
        if (favorite == null)
            return Result.Failure(FavoriteErrors.NotFound(favoriteId));

        if (favorite.UserId != Id)
            return Result.Failure(FavoriteErrors.CannotModifyOtherFavorites);

        var result = favorite.Remove();
        if (result.IsFailure)
            return result;

        UpdatedAt = DateTime.UtcNow;
        //77AddDomainEvent(new UserFavoriteRemovedEvent(Id, favorite.TargetUserId));

        return Result.Success();
    }

    /// <summary>
    /// Проверяет, находится ли пользователь в избранном
    /// </summary>
    public bool IsFavorite(Guid targetUserId)
    {
        return _favorites.Any(f => f.TargetUserId == targetUserId && f.IsActive);
    }

    /// <summary>
    /// Получает список ID избранных пользователей
    /// </summary>
    public IReadOnlyList<Guid> GetFavoriteUserIds()
    {
        return _favorites
            .Where(f => f.IsActive)
            .Select(f => f.TargetUserId)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Получает список активных избранных
    /// </summary>
    public IReadOnlyList<UserFavorite> GetActiveFavorites()
    {
        return _favorites
            .Where(f => f.IsActive)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Получает все избранные (включая неактивные)
    /// </summary>
    public IReadOnlyList<UserFavorite> GetAllFavorites()
    {
        return _favorites
            .OrderByDescending(f => f.AddedAt)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Получает количество избранных
    /// </summary>
    public int GetFavoritesCount()
    {
        return _favorites.Count(f => f.IsActive);
    }

    /// <summary>
    /// Проверяет, есть ли избранные
    /// </summary>
    public bool HasFavorites()
    {
        return _favorites.Any(f => f.IsActive);
    }

    /// <summary>
    /// Проверяет, можно ли добавить еще избранных
    /// </summary>
    public bool CanAddMoreFavorites()
    {
        return _favorites.Count(f => f.IsActive) < 100;
    }

    /// <summary>
    /// Получает количество свободных мест в избранном
    /// </summary>
    public int GetRemainingFavoriteSlots()
    {
        return Math.Max(0, 100 - _favorites.Count(f => f.IsActive));
    }

    // === SUBSCRIPTION MANAGEMENT ===

    /// <summary>
    /// Подписывается на пользователя
    /// </summary>
    public Result SubscribeTo(Guid targetUserId, SubscriptionType type)
    {
        if (Id == targetUserId)
            return Result.Failure(SubscriptionErrors.CannotSubscribeYourself);

        if (!CanParticipate())
            return Result.Failure(SubscriptionErrors.CannotSubscribeInactiveUser);

        if (!Enum.IsDefined(typeof(SubscriptionType), type))
            return Result.Failure(SubscriptionErrors.InvalidSubscriptionType);

        var existing = _subscriptions.FirstOrDefault(s =>
            s.TargetUserId == targetUserId &&
            s.Type == type &&
            s.IsActive);

        if (existing != null)
            return Result.Failure(SubscriptionErrors.AlreadyExists(targetUserId, type));

        if (_subscriptions.Count(s => s.IsActive) >= 100)
            return Result.Failure(SubscriptionErrors.TooManySubscriptions(100));

        var subscription = UserSubscription.Create(Id, targetUserId, type);
        if (subscription.IsFailure)
            return Result.Failure(subscription.Error);

        _subscriptions.Add(subscription.Value);
        UpdatedAt = DateTime.UtcNow;
        //7AddDomainEvent(new UserSubscribedEvent(Id, targetUserId, type));

        return Result.Success();
    }

    /// <summary>
    /// Отписывается от пользователя
    /// </summary>
    public Result UnsubscribeFrom(Guid targetUserId, SubscriptionType type)
    {
        var subscription = _subscriptions.FirstOrDefault(s =>
            s.TargetUserId == targetUserId &&
            s.Type == type &&
            s.IsActive);

        if (subscription == null)
            return Result.Failure(SubscriptionErrors.NotFound(Id, targetUserId, type));

        subscription.Unsubscribe();
        UpdatedAt = DateTime.UtcNow;
        //7AddDomainEvent(new UserUnsubscribedEvent(Id, targetUserId, type));

        return Result.Success();
    }

    /// <summary>
    /// Проверяет подписку на пользователя
    /// </summary>
    public bool IsSubscribedTo(Guid targetUserId, SubscriptionType? type = null)
    {
        return _subscriptions.Any(s =>
            s.TargetUserId == targetUserId &&
            s.IsActive &&
            (!type.HasValue || s.Type == type));
    }

    /// <summary>
    /// Получает подписки
    /// </summary>
    public IReadOnlyList<UserSubscription> GetSubscriptions(SubscriptionType? type = null)
    {
        return _subscriptions
            .Where(s => s.IsActive && (!type.HasValue || s.Type == type))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Получает ID пользователей, на которых подписан
    /// </summary>
    public IReadOnlyList<Guid> GetSubscribedUserIds(SubscriptionType? type = null)
    {
        return GetSubscriptions(type)
            .Select(s => s.TargetUserId)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Получает количество подписок
    /// </summary>
    public int GetSubscriptionsCount(SubscriptionType? type = null)
    {
        return _subscriptions.Count(s =>
            s.IsActive &&
            (!type.HasValue || s.Type == type));
    }

    // === PROFILE MANAGEMENT ===

    /// <summary>
    /// Обновляет профиль
    /// </summary>
    public Result UpdateProfile(UserProfile newProfile)
    {
        if (newProfile == null)
            return Result.Failure(ProfileErrors.ProfileUpdateFailed("Profile cannot be null"));

        if (Status == UserStatus.Deleted)
            return Result.Failure(ProfileErrors.CannotUpdateDeletedUser);

        if (Status == UserStatus.Suspended)
            return Result.Failure(ProfileErrors.CannotUpdateSuspendedUser);

        var oldProfile = Profile;
        Profile = newProfile;

        // Проверяем заполненность профиля
        if (!ProfileCompleted && newProfile.IsProfileComplete())
        {
            ProfileCompleted = true;
            //77AddDomainEvent(new UserProfileCompletedEvent(Id));

            // Если статус Inactive и профиль заполнен - активируем
            if (Status == UserStatus.Inactive)
            {
                ActivateAfterProfileComplete();
            }
        }

        UpdatedAt = DateTime.UtcNow;
        //77AddDomainEvent(new UserProfileUpdatedEvent(Id, oldProfile, newProfile));

        return Result.Success();
    }

    /// <summary>
    /// Обновляет основную информацию
    /// </summary>
    public Result UpdateBasicInfo(
        string? bio,
        DateTime? dateOfBirth,
        string? gender)
    {
        if (Status == UserStatus.Deleted)
            return Result.Failure(ProfileErrors.CannotUpdateDeletedUser);

        var newProfile = Profile.UpdateBasicInfo(
            bio ?? Profile.Bio,
            dateOfBirth ?? Profile.DateOfBirth,
            gender ?? Profile.Gender);

        return UpdateProfile(newProfile);
    }

    /// <summary>
    /// Обновляет контакты
    /// </summary>
    public Result UpdateContacts(
        string? phone,
        string? telegram,
        string? instagram)
    {
        if (Status == UserStatus.Deleted)
            return Result.Failure(ProfileErrors.CannotUpdateDeletedUser);

        var newProfile = Profile.UpdateContacts(
            phone ?? Profile.Phone,
            telegram ?? Profile.Telegram,
            instagram ?? Profile.Instagram);

        return UpdateProfile(newProfile);
    }

    /// <summary>
    /// Обновляет местоположение
    /// </summary>
    public Result UpdateLocation(
        string? city,
        string? country,
        double? latitude,
        double? longitude,
        string? timeZone)
    {
        if (Status == UserStatus.Deleted)
            return Result.Failure(ProfileErrors.CannotUpdateDeletedUser);

        var newProfile = Profile.UpdateLocation(
            city ?? Profile.City,
            country ?? Profile.Country,
            latitude ?? Profile.Latitude,
            longitude ?? Profile.Longitude,
            timeZone ?? Profile.TimeZone);

        return UpdateProfile(newProfile);
    }

    /// <summary>
    /// Обновляет социальные характеристики
    /// </summary>
    public Result UpdateSocial(
        string? gender,
        string? languages)
    {
        if (Status == UserStatus.Deleted)
            return Result.Failure(ProfileErrors.CannotUpdateDeletedUser);

        var newProfile = Profile.UpdateSocial(
            gender ?? Profile.Gender,
            languages ?? Profile.Languages);

        return UpdateProfile(newProfile);
    }

    /// <summary>
    /// Обновляет предпочтения активностей
    /// </summary>
    public Result UpdateActivityPreferences(ActivityPreferences preferences)
    {
        if (Status == UserStatus.Deleted)
            return Result.Failure(ActivityPreferencesErrors.UpdateFailed("Cannot update preferences of deleted user"));

        var newProfile = Profile.UpdateActivityPreferences(preferences);
        return UpdateProfile(newProfile);
    }

    public Result UpdateMaxDistance(int? maxDistanceKm)
    {
        if (Status == UserStatus.Deleted)
            return Result.Failure(ActivityPreferencesErrors.UpdateFailed("Cannot update preferences of deleted user"));

        if (maxDistanceKm.HasValue && (maxDistanceKm < 1 || maxDistanceKm > 500))
            return Result.Failure(ActivityPreferencesErrors.InvalidMaxDistance);

        var currentPrefs = Profile.ActivityPreferences;
        var newPreferences = ActivityPreferences.Create(
            preferredActivities: currentPrefs.PreferredActivities,
            activityLevels: currentPrefs.ActivityLevels,
            preferredTimeOfDay: currentPrefs.PreferredTimeOfDay,
            preferredDays: currentPrefs.PreferredDays,
            maxDistanceKm: maxDistanceKm,
            minParticipants: currentPrefs.MinParticipants,
            maxParticipants: currentPrefs.MaxParticipants
        );

        if (newPreferences.IsFailure)
            return Result.Failure(newPreferences.Error);

        return UpdateActivityPreferences(newPreferences.Value);
    }

    public Result UpdateParticipantsRange(int? minParticipants, int? maxParticipants)
    {
        if (Status == UserStatus.Deleted)
            return Result.Failure(ActivityPreferencesErrors.UpdateFailed("Cannot update preferences of deleted user"));

        if (minParticipants.HasValue && minParticipants < 1)
            return Result.Failure(ActivityPreferencesErrors.InvalidMinParticipants);

        if (maxParticipants.HasValue && maxParticipants < 1)
            return Result.Failure(ActivityPreferencesErrors.InvalidMaxParticipants);

        if (minParticipants.HasValue && maxParticipants.HasValue &&
            minParticipants > maxParticipants)
            return Result.Failure(ActivityPreferencesErrors.MinParticipantsGreaterThanMax);

        var currentPrefs = Profile.ActivityPreferences;
        var newPreferences = ActivityPreferences.Create(
            preferredActivities: currentPrefs.PreferredActivities,
            activityLevels: currentPrefs.ActivityLevels,
            preferredTimeOfDay: currentPrefs.PreferredTimeOfDay,
            preferredDays: currentPrefs.PreferredDays,
            maxDistanceKm: currentPrefs.MaxDistanceKm,
            minParticipants: minParticipants,
            maxParticipants: maxParticipants
        );

        if (newPreferences.IsFailure)
            return Result.Failure(newPreferences.Error);

        return UpdateActivityPreferences(newPreferences.Value);
    }

    /// <summary>
    /// Обновляет статус доступности
    /// </summary>
    public Result UpdateAvailability(AvailabilityStatus newStatus)
    {
        if (Status == UserStatus.Deleted)
            return Result.Failure(ProfileErrors.CannotUpdateDeletedUser);

        var newProfile = Profile.UpdateAvailability(newStatus);
        return UpdateProfile(newProfile);
    }

    /// <summary>
    /// Обновляет настройки приватности
    /// </summary>
    public Result UpdatePrivacy(bool isPublic)
    {
        if (Status == UserStatus.Deleted)
            return Result.Failure(ProfileErrors.CannotUpdateDeletedUser);

        var newProfile = Profile.UpdatePrivacy(isPublic);
        return UpdateProfile(newProfile);
    }

    /// <summary>
    /// Проверяет, заполнен ли профиль
    /// </summary>
    public bool IsProfileComplete()
    {
        return ProfileCompleted && Profile.IsProfileComplete();
    }

    /// <summary>
    /// Получает отображаемое имя
    /// </summary>
    public string GetDisplayName()
    {
        return !string.IsNullOrEmpty(Username) ? Username : "User";
    }

    /// <summary>
    /// Получает возраст
    /// </summary>
    public int GetAge()
    {
        return Profile.GetAge();
    }

    /// <summary>
    /// Проверяет, находится ли пользователь рядом
    /// </summary>
    public bool IsNearLocation(double targetLat, double targetLng, double maxDistanceKm)
    {
        return Profile.IsNearLocation(targetLat, targetLng, maxDistanceKm);
    }

    /// <summary>
    /// Проверяет, говорит ли на языке
    /// </summary>
    public bool SpeaksLanguage(string language)
    {
        return Profile.SpeaksLanguage(language);
    }

    /// <summary>
    /// Обновляет время последней активности
    /// </summary>
    public void UpdateLastActive()
    {
        LastActiveAt = DateTime.UtcNow;
    }

    // === STATUS MANAGEMENT ===

    public Result Activate(string? reason = null)
    {
        if (Status == UserStatus.Active)
            return Result.Failure(StatusErrors.AlreadyActive);

        if (Status == UserStatus.Deleted)
            return Result.Failure(StatusErrors.CannotActivateDeleted);

        if (Status == UserStatus.Suspended)
            return Result.Failure(StatusErrors.CannotActivateSuspended);

        if (!ProfileCompleted || !Profile.IsProfileComplete())
            return Result.Failure(StatusErrors.CannotActivateWithoutProfile);

        Status = UserStatus.Active;
        LastStatusChangeAt = DateTime.UtcNow;
        StatusReason = reason;
        UpdatedAt = DateTime.UtcNow;

        //77AddDomainEvent(new UserActivatedEvent(Id, reason));
        return Result.Success();
    }

    public Result ActivateAfterProfileComplete()
    {
        if (!ProfileCompleted || !Profile.IsProfileComplete())
            return Result.Failure(StatusErrors.CannotActivateWithoutProfile);

        if (Status == UserStatus.Active)
            return Result.Success();

        if (Status == UserStatus.Deleted)
            return Result.Failure(StatusErrors.CannotActivateDeleted);

        if (Status == UserStatus.Suspended)
            return Result.Failure(StatusErrors.CannotActivateSuspended);

        Status = UserStatus.Active;
        LastStatusChangeAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        //77AddDomainEvent(new UserActivatedEvent(Id, "Profile completed"));
        return Result.Success();
    }

    public Result Suspend(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(StatusErrors.SuspensionReasonRequired);

        if (reason.Length > 500)
            return Result.Failure(StatusErrors.StatusReasonTooLong(500));

        if (Status == UserStatus.Suspended)
            return Result.Failure(StatusErrors.AlreadySuspended);

        if (Status == UserStatus.Deleted)
            return Result.Failure(StatusErrors.CannotSuspendDeleted);

        if (Status == UserStatus.Inactive)
            return Result.Failure(StatusErrors.CannotSuspendInactive);

        // Проверка бизнес-условий
        if (HasActiveEvents())
            return Result.Failure(StatusErrors.CannotSuspendActiveUserWithActiveEvents);

        if (HasActiveBookings())
            return Result.Failure(StatusErrors.CannotSuspendActiveUserWithActiveBookings);

        var oldStatus = Status;
        Status = UserStatus.Suspended;
        LastStatusChangeAt = DateTime.UtcNow;
        StatusReason = reason;
        UpdatedAt = DateTime.UtcNow;

        //77AddDomainEvent(new UserSuspendedEvent(Id, reason, oldStatus));
        return Result.Success();
    }

    public Result Unsuspend(string? reason = null)
    {
        if (Status != UserStatus.Suspended)
            return Result.Failure(StatusErrors.CannotUnsuspendActive);

        if (reason?.Length > 500)
            return Result.Failure(StatusErrors.StatusReasonTooLong(500));

        Status = UserStatus.Active;
        LastStatusChangeAt = DateTime.UtcNow;
        StatusReason = reason;
        UpdatedAt = DateTime.UtcNow;

        //77AddDomainEvent(new UserUnsuspendedEvent(Id, reason));
        return Result.Success();
    }

    public Result Delete(string? reason = null)
    {
        if (Status == UserStatus.Deleted)
            return Result.Failure(StatusErrors.AlreadyDeleted);

        if (Status == UserStatus.Suspended)
            return Result.Failure(StatusErrors.CannotDeleteSuspended);

        if (Status == UserStatus.Active && string.IsNullOrWhiteSpace(reason))
            return Result.Failure(StatusErrors.CannotDeleteActiveWithoutReason);

        if (reason?.Length > 500)
            return Result.Failure(StatusErrors.StatusReasonTooLong(500));

        // Проверка бизнес-условий
        if (HasActiveEvents())
            return Result.Failure(StatusErrors.CannotDeleteUserWithActiveEvents);

        if (HasActiveBookings())
            return Result.Failure(StatusErrors.CannotDeleteUserWithActiveBookings);

        Status = UserStatus.Deleted;
        LastStatusChangeAt = DateTime.UtcNow;
        StatusReason = reason;
        UpdatedAt = DateTime.UtcNow;

        //77AddDomainEvent(new UserDeletedEvent(Id, reason));
        return Result.Success();
    }

    public Result MarkInactive(string? reason = null)
    {
        if (Status == UserStatus.Deleted)
            return Result.Failure(StatusErrors.CannotMarkDeletedAsInactive);

        if (Status == UserStatus.Suspended)
            return Result.Failure(StatusErrors.CannotMarkSuspendedAsInactive);

        if (Status == UserStatus.Inactive)
            return Result.Success();

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(StatusErrors.CannotMarkUserInactiveWithoutReason);

        if (reason.Length > 500)
            return Result.Failure(StatusErrors.StatusReasonTooLong(500));

        // Проверка бизнес-условий
        if (HasActiveEvents())
            return Result.Failure(StatusErrors.CannotMarkUserInactiveWithActiveEvents);

        if (HasActiveBookings())
            return Result.Failure(StatusErrors.CannotMarkUserInactiveWithActiveBookings);

        Status = UserStatus.Inactive;
        LastStatusChangeAt = DateTime.UtcNow;
        StatusReason = reason;
        UpdatedAt = DateTime.UtcNow;

        //77AddDomainEvent(new UserMarkedInactiveEvent(Id, reason));
        return Result.Success();
    }

    // === CHECK METHODS ===

    /// <summary>
    /// Проверяет, может ли пользователь войти
    /// </summary>
    public bool CanLogin()
    {
        return Status == UserStatus.Active;
    }

    /// <summary>
    /// Проверяет, может ли пользователь участвовать в активностях
    /// </summary>
    public bool CanParticipate()
    {
        return Status == UserStatus.Active;
    }

    /// <summary>
    /// Проверяет, может ли пользователь быть найден в поиске
    /// </summary>
    public bool CanBeSearched()
    {
        return Status == UserStatus.Active && Profile.IsPublic;
    }

    /// <summary>
    /// Проверяет, активен ли пользователь
    /// </summary>
    public bool IsActiveUser()
    {
        return Status == UserStatus.Active;
    }

    /// <summary>
    /// Проверяет, заблокирован ли пользователь
    /// </summary>
    public bool IsSuspended()
    {
        return Status == UserStatus.Suspended;
    }

    /// <summary>
    /// Проверяет, удален ли пользователь
    /// </summary>
    public bool IsDeleted()
    {
        return Status == UserStatus.Deleted;
    }

    /// <summary>
    /// Проверяет, неактивен ли пользователь
    /// </summary>
    public bool IsInactive()
    {
        return Status == UserStatus.Inactive;
    }

    // === PRIVATE METHODS ===

    private void UpdateProfileInterests()
    {
        var activeTags = GetActiveTags();
        var interests = activeTags.Any()
            ? string.Join(",", activeTags.Select(t => t.Name))
            : null;

        Profile = Profile.UpdateInterests(interests);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public Result AssignRole(Role role)
    {
        if (_roles.Any(r => r.Name == role.Name))
        {
            return Result.Success(); // Already assigned
        }

        _roles.Add(role);
        return Result.Success();
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

    private bool HasActiveEvents()
    {
        // Проверка наличия активных событий
        return false; // Реализация позже
    }

    private bool HasActiveBookings()
    {
        // Проверка наличия активных бронирований
        return false; // Реализация позже
    }

}
