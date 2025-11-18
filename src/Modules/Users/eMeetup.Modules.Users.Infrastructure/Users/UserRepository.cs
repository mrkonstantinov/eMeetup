using eMeetup.Modules.Users.Domain.Users;
using eMeetup.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace eMeetup.Modules.Users.Infrastructure.Users;

internal sealed class UserRepository(UsersDbContext context) : IUserRepository
{
    //public async Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    //{
    //    return await context.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
    //}

    //public void Insert(User user)
    //{
    //    foreach (Role role in user.Roles)
    //    {
    //        context.Attach(role);
    //    }

    //    context.Users.Add(user);
    //}
    public Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Insert(User user)
    {
        foreach (Role role in user.Roles)
        {
            context.Attach(role);
        }

        if (user?.Interests?.Any() == true)
            foreach (var interest in user.Interests)
            {
                context.Attach(interest);
            }

        if (user?.Photos?.Any() == true)
        {
            foreach (var photo in user.Photos)
            {
                context.Attach(photo);
            }
        }
        context.Users.Add(user);
    }
}
