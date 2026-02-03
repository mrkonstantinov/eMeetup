using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Users;
using eMeetup.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Infrastructure.Users;

internal sealed class UserRepository(UsersDbContext context, ILogger<UserRepository> logger) : IUserRepository
{
    private readonly UsersDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogger<UserRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Photos)
            .Include(u => u.Roles)
            .Include(u => u.Interests)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await _context.Users
            .Include(u => u.Photos)
            .Include(u => u.Roles)
            .Include(u => u.Interests)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty", nameof(username));

        var normalizedUsername = username.Trim().ToLowerInvariant();

        return await _context.Users
            .Include(u => u.Photos)
            .Include(u => u.Roles)
            .Include(u => u.Interests)
            .FirstOrDefaultAsync(u => u.UserName.ToLower() == normalizedUsername, cancellationToken);
    }

    public async Task<User?> GetByIdWithPhotosAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Users
                .Include(u => u.Photos) // This is the key - eager load photos
                .AsSplitQuery() // Optional: Better performance for complex queries
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user with photos by ID: {UserId}", id);
            throw;
        }
    }
    public void Insert(User user)
    {
        foreach (Role role in user.Roles)
        {
            context.Attach(role);
        }

        _context.Users.Add(user);
    }

    // Optional helper methods
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await _context.Users
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty", nameof(username));

        var normalizedUsername = username.Trim().ToLowerInvariant();
        return await _context.Users
            .AnyAsync(u => u.UserName.ToLower() == normalizedUsername, cancellationToken);
    }

    public async Task<User?> GetByIdentityIdAsync(Guid identityId, CancellationToken cancellationToken)
    {
        // Assuming IdentityId might be stored as a string representation of Guid
        var identityIdString = identityId.ToString();

        return await context.Users
            .Include(u => u.Photos)
            .SingleOrDefaultAsync(u => u.IdentityId == identityIdString, cancellationToken);

        try
        {
            return await _context.Users
                .Include(u => u.Photos)
                .Include(u => u.Roles)
                .Include(u => u.Interests)
                .ThenInclude(ui => ui.Tag)
                //77.AsSplitQuery()
                .FirstOrDefaultAsync(u => u.IdentityId == identityIdString, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by identity ID: {IdentityId}", identityId);
            throw;
        }
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        // If the entity is already being tracked, just mark it as modified
        var entry = _context.Entry(user);

        if (entry.State == EntityState.Detached)
        {
            // Try to find if the entity is already in the context
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

            if (existingUser != null)
            {
                // Update the existing entity with the new values
                _context.Entry(existingUser).CurrentValues.SetValues(user);
            }
            else
            {
                // Attach as modified
                _context.Users.Update(user);
            }
        }
        else
        {
            // Entity is already tracked, just mark as modified
            entry.State = EntityState.Modified;
        }
    }

    //public void Insert(User user)
    //{
    //    foreach (Role role in user.Roles)
    //    {
    //        context.Attach(role);
    //    }

    //    if (user?.Interests?.Any() == true)
    //        foreach (var interest in user.Interests)
    //        {
    //            context.Attach(interest);
    //        }

    //    if (user?.Photos?.Any() == true)
    //    {
    //        foreach (var photo in user.Photos)
    //        {
    //            context.Attach(photo);
    //        }
    //    }
    //    context.Users.Add(user);
    //}
}
