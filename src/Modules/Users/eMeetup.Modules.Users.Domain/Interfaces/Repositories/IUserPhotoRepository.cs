using System.Data;
using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Domain.Interfaces.Repositories;

public interface IUserPhotoRepository
{
    Task AddAsync(UserPhoto photo, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<UserPhoto> photos, CancellationToken cancellationToken = default);
    Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserPhoto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserPhoto?> GetByIdWithTrackingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<UserPhoto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<UserPhoto>> GetByUserIdWithTrackingAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetNextDisplayOrderAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserPhoto?> GetPhotoByDisplayOrderAsync(Guid userId, int displayOrder, CancellationToken cancellationToken = default);
    Task<UserPhoto?> GetPrimaryPhotoAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserPhoto?> GetPrimaryPhotoOrDefaultAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<string?> GetPrimaryPhotoUrlAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserPhoto?> GetPrimaryPhotoWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasPhotosAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasPrimaryPhotoAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsPhotoPrimaryAsync(Guid photoId, CancellationToken cancellationToken = default);
    Task MarkAllAsSecondaryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task ReorderPhotosAsync(Guid userId, Dictionary<Guid, int> photoOrderMap, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserPhoto photo, CancellationToken cancellationToken = default);
    Task UpdatePrimaryPhotoAsync(Guid userId, Guid newPrimaryPhotoId, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<UserPhoto> photos, CancellationToken cancellationToken = default);
}
