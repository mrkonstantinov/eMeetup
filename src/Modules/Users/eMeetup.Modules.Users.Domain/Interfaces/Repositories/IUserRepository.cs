using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    // === GET BY ID ===
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByIdWithPhotosAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByIdWithTagsAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByIdWithFavoritesAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByIdWithSubscriptionsAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByIdWithAllAsync(Guid id, CancellationToken ct = default);

    // === GET BY IDENTITY ID (Keycloak) ===
    Task<User?> GetByIdentityIdAsync(Guid identityId, CancellationToken ct = default);
    Task<User?> GetByIdentityIdWithAllAsync(Guid identityId, CancellationToken ct = default);

    // === GET BY EMAIL / USERNAME ===
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);

    // === EXISTS ===
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);
    Task<bool> ExistsByIdentityIdAsync(Guid identityId, CancellationToken ct = default);

    // === CRUD ===
    void Insert(User user);
    void Update(User user);
    void Remove(User user);

    // === GET MANY ===
    Task<List<User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<List<User>> GetActiveUsersAsync(CancellationToken ct = default);
    Task<List<User>> GetUsersInactiveSinceAsync(DateTime since, CancellationToken ct = default);
}
