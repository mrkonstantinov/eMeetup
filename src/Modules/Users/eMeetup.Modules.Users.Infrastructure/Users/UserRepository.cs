using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Users;
using eMeetup.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace eMeetup.Modules.Users.Infrastructure.Users;

internal sealed class UserRepository : IUserRepository
{
    private readonly UsersDbContext _context;

    public UserRepository(UsersDbContext context)
    {
        _context = context;
    }

    // ================================================================
    // === GET BY ID ===
    // ================================================================

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // Owned collections (Profile, _photos) загружаются автоматически
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByIdWithPhotosAsync(Guid id, CancellationToken ct = default)
    {
        // Owned collection (_photos) загружается автоматически
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByIdWithTagsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users
            .Include(u => u.UserTags)
                .ThenInclude(ut => ut.TagGroup)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByIdWithFavoritesAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users
            .Include(u => u.Favorites)
            .Include(u => u.FavoritedBy)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByIdWithSubscriptionsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users
            .Include(u => u.Subscriptions)
            .Include(u => u.Subscribers)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByIdWithAllAsync(Guid id, CancellationToken ct = default)
    {
        // Owned collections (Profile, _photos) загружаются автоматически
        // Обычные навигации — через Include
        return await _context.Users
            .Include(u => u.UserTags)
                .ThenInclude(ut => ut.TagGroup)
            .Include(u => u.Favorites)
            .Include(u => u.FavoritedBy)
            .Include(u => u.Subscriptions)
            .Include(u => u.Subscribers)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    // ================================================================
    // === GET BY IDENTITY ID (Keycloak) ===
    // ================================================================

    public async Task<User?> GetByIdentityIdAsync(Guid identityId, CancellationToken ct = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.IdentityId == identityId, ct);
    }

    public async Task<User?> GetByIdentityIdWithAllAsync(Guid identityId, CancellationToken ct = default)
    {
        return await _context.Users
            .Include(u => u.UserTags)
                .ThenInclude(ut => ut.TagGroup)
            .Include(u => u.Favorites)
            .Include(u => u.FavoritedBy)
            .Include(u => u.Subscriptions)
            .Include(u => u.Subscribers)
            .FirstOrDefaultAsync(u => u.IdentityId == identityId, ct);
    }

    // ================================================================
    // === GET BY EMAIL / USERNAME ===
    // ================================================================

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username, ct);
    }

    // ================================================================
    // === EXISTS ===
    // ================================================================

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Id == id, ct);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email, ct);
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Username == username, ct);
    }

    public async Task<bool> ExistsByIdentityIdAsync(Guid identityId, CancellationToken ct = default)
    {
        return await _context.Users
            .AnyAsync(u => u.IdentityId == identityId, ct);
    }

    // ================================================================
    // === CRUD ===
    // ================================================================

    public void Insert(User user)
    {
        _context.Users.Add(user);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public void Remove(User user)
    {
        _context.Users.Remove(user);
    }

    // ================================================================
    // === GET MANY ===
    // ================================================================

    public async Task<List<User>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken ct = default)
    {
        var idList = ids.ToList();

        return await _context.Users
            .Where(u => idList.Contains(u.Id))
            .ToListAsync(ct);
    }

    public async Task<List<User>> GetActiveUsersAsync(CancellationToken ct = default)
    {
        return await _context.Users
            .Where(u => u.Status == UserStatus.Active)
            .ToListAsync(ct);
    }

    public async Task<List<User>> GetUsersInactiveSinceAsync(
        DateTime since,
        CancellationToken ct = default)
    {
        return await _context.Users
            .Where(u => u.LastActiveAt.HasValue && u.LastActiveAt < since)
            .Where(u => u.Status == UserStatus.Active)
            .ToListAsync(ct);
    }
}
