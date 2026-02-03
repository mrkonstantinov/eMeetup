using System.Threading.Tasks;
using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    // Add these methods for proper duplicate checking
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithPhotosAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByIdentityIdAsync(Guid identityId, CancellationToken cancellationToken);

    void Insert(User user);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}
