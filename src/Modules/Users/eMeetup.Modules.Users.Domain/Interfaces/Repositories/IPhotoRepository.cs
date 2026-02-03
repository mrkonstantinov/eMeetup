using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Domain.Interfaces.Repositories;

public interface IPhotoRepository
{
    Task<Result> AddPhotoAsync(Guid userId, string url, bool isPrimary = false, CancellationToken cancellationToken = default);
    Task<Result> AddPhotosAsync(Guid userId, IEnumerable<string> urls, CancellationToken cancellationToken = default);
    Task<Result> SetPrimaryPhotoAsync(Guid userId, Guid photoId, CancellationToken cancellationToken = default);
    Task<Result> RemovePhotoAsync(Guid userId, Guid photoId, CancellationToken cancellationToken = default);
    Task<Result> ReorderPhotosAsync(Guid userId, Dictionary<Guid, int> photoOrders, CancellationToken cancellationToken = default);
    Task<List<UserPhoto>> GetUserPhotosAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserPhoto?> GetPhotoAsync(Guid photoId, CancellationToken cancellationToken = default);
}
