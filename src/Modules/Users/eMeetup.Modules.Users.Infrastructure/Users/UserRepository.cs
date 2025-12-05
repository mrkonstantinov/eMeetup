using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Users;
using eMeetup.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace eMeetup.Modules.Users.Infrastructure.Users;

internal sealed class UserRepository(UsersDbContext context) : IUserRepository
{
    private readonly UsersDbContext _context = context;

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
