using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using eMeetup.Common.Application.Exceptions;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Users;
using eMeetup.Modules.Users.Infrastructure.Database;
using eMeetup.Modules.Users.Infrastructure.Users;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using ConcurrencyException = eMeetup.Common.Application.Exceptions.ConcurrencyException;

namespace eMeetup.Modules.Users.Infrastructure.Photos;

//internal interface IUserPhotoRepository
//{
//    Task AddAsync(UserPhoto photo, CancellationToken cancellationToken = default);
//    Task AddRangeAsync(IEnumerable<UserPhoto> photos, CancellationToken cancellationToken = default);
//    Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
//    Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
//    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
//    Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
//    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
//    Task<UserPhoto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
//    Task<UserPhoto?> GetByIdWithTrackingAsync(Guid id, CancellationToken cancellationToken = default);
//    Task<List<UserPhoto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
//    Task<List<UserPhoto>> GetByUserIdWithTrackingAsync(Guid userId, CancellationToken cancellationToken = default);
//    Task<int> GetNextDisplayOrderAsync(Guid userId, CancellationToken cancellationToken = default);
//    Task<UserPhoto?> GetPhotoByDisplayOrderAsync(Guid userId, int displayOrder, CancellationToken cancellationToken = default);
//    Task<UserPhoto?> GetPrimaryPhotoAsync(Guid userId, CancellationToken cancellationToken = default);
//    Task<UserPhoto?> GetPrimaryPhotoOrDefaultAsync(Guid userId, CancellationToken cancellationToken = default);
//    Task<string?> GetPrimaryPhotoUrlAsync(Guid userId, CancellationToken cancellationToken = default);
//    Task<UserPhoto?> GetPrimaryPhotoWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default);
//    Task<bool> HasPhotosAsync(Guid userId, CancellationToken cancellationToken = default);
//    Task<bool> HasPrimaryPhotoAsync(Guid userId, CancellationToken cancellationToken = default);
//    Task<bool> IsPhotoPrimaryAsync(Guid photoId, CancellationToken cancellationToken = default);
//    Task MarkAllAsSecondaryAsync(Guid userId, CancellationToken cancellationToken = default);
//    Task ReorderPhotosAsync(Guid userId, Dictionary<Guid, int> photoOrderMap, CancellationToken cancellationToken = default);
//    Task UpdateAsync(UserPhoto photo, CancellationToken cancellationToken = default);
//    Task UpdatePrimaryPhotoAsync(Guid userId, Guid newPrimaryPhotoId, CancellationToken cancellationToken = default);
//    Task UpdateRangeAsync(IEnumerable<UserPhoto> photos, CancellationToken cancellationToken = default);
//}

internal sealed class UserPhotoRepository(UsersDbContext context, ILogger<UserRepository> logger) : IUserPhotoRepository
{
    private readonly UsersDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogger<UserRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    // Core CRUD Operations
    public async Task<List<UserPhoto>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.UserPhotos
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get photos for user {UserId}", userId);
            throw new RepositoryException("Failed to retrieve user photos", ex);
        }
    }

    public async Task<UserPhoto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.UserPhotos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get photo by id {PhotoId}", id);
            throw new RepositoryException($"Failed to retrieve photo with id {id}", ex);
        }
    }

    public async Task AddAsync(
        UserPhoto photo,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.UserPhotos.AddAsync(photo, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Added new photo {PhotoId} for user {UserId}",
                photo.Id, photo.UserId);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error adding photo for user {UserId}", photo.UserId);
            throw new RepositoryException("Failed to add photo due to database constraint", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add photo for user {UserId}", photo.UserId);
            throw new RepositoryException("Failed to add photo", ex);
        }
    }

    public async Task UpdateAsync(
        UserPhoto photo,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _context.UserPhotos.Update(photo);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Updated photo {PhotoId}", photo.Id);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency conflict updating photo {PhotoId}", photo.Id);
            throw new ConcurrencyException("Photo was modified by another user", ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error updating photo {PhotoId}", photo.Id);
            throw new RepositoryException("Failed to update photo due to database constraint", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update photo {PhotoId}", photo.Id);
            throw new RepositoryException("Failed to update photo", ex);
        }
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var photo = await _context.UserPhotos.FindAsync(id);
            if (photo != null)
            {
                _context.UserPhotos.Remove(photo);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogDebug("Deleted photo {PhotoId}", id);
            }
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error deleting photo {PhotoId}", id);
            throw new RepositoryException("Failed to delete photo due to database constraint", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete photo {PhotoId}", id);
            throw new RepositoryException("Failed to delete photo", ex);
        }
    }

    // Primary Photo Operations
    public async Task<UserPhoto?> GetPrimaryPhotoAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.UserPhotos
                .AsNoTracking()
                .Where(p => p.UserId == userId && p.IsPrimary)
                .OrderBy(p => p.DisplayOrder)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get primary photo for user {UserId}", userId);
            throw new RepositoryException($"Failed to retrieve primary photo for user {userId}", ex);
        }
    }

    public async Task<UserPhoto?> GetPrimaryPhotoWithDetailsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.UserPhotos
                .AsNoTracking()
                .Where(p => p.UserId == userId && p.IsPrimary)
                .OrderBy(p => p.DisplayOrder)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get primary photo with details for user {UserId}", userId);
            throw new RepositoryException($"Failed to retrieve primary photo details for user {userId}", ex);
        }
    }

    public async Task<string?> GetPrimaryPhotoUrlAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var photo = await GetPrimaryPhotoAsync(userId, cancellationToken);
            return photo?.Url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get primary photo URL for user {UserId}", userId);
            throw new RepositoryException($"Failed to retrieve primary photo URL for user {userId}", ex);
        }
    }

    public async Task<bool> IsPhotoPrimaryAsync(
        Guid photoId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.UserPhotos
                .AsNoTracking()
                .AnyAsync(p => p.Id == photoId && p.IsPrimary, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if photo {PhotoId} is primary", photoId);
            throw new RepositoryException($"Failed to check primary status for photo {photoId}", ex);
        }
    }

    // Additional Utility Methods (not in interface but useful)
    public async Task<List<UserPhoto>> GetByUserIdWithTrackingAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.UserPhotos
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get photos with tracking for user {UserId}", userId);
            throw new RepositoryException("Failed to retrieve user photos with tracking", ex);
        }
    }

    public async Task<UserPhoto?> GetByIdWithTrackingAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.UserPhotos
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get photo by id {PhotoId} with tracking", id);
            throw new RepositoryException($"Failed to retrieve photo with id {id} with tracking", ex);
        }
    }

    public async Task AddRangeAsync(
        IEnumerable<UserPhoto> photos,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.UserPhotos.AddRangeAsync(photos, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Added {Count} photos", photos.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add photos");
            throw new RepositoryException("Failed to add photos", ex);
        }
    }

    public async Task UpdateRangeAsync(
        IEnumerable<UserPhoto> photos,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _context.UserPhotos.UpdateRange(photos);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Updated {Count} photos", photos.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update photos");
            throw new RepositoryException("Failed to update photos", ex);
        }
    }

    public async Task DeleteRangeAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var photos = await _context.UserPhotos
                .Where(p => ids.Contains(p.Id))
                .ToListAsync(cancellationToken);

            if (photos.Any())
            {
                _context.UserPhotos.RemoveRange(photos);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogDebug("Deleted {Count} photos", photos.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete photos");
            throw new RepositoryException("Failed to delete photos", ex);
        }
    }

    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.UserPhotos
                .AnyAsync(p => p.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if photo {PhotoId} exists", id);
            throw new RepositoryException($"Failed to check existence of photo {id}", ex);
        }
    }

    public async Task<int> CountByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.UserPhotos
                .CountAsync(p => p.UserId == userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count photos for user {UserId}", userId);
            throw new RepositoryException($"Failed to count photos for user {userId}", ex);
        }
    }

    public async Task<int> GetNextDisplayOrderAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var maxDisplayOrder = await _context.UserPhotos
                .Where(p => p.UserId == userId)
                .MaxAsync(p => (int?)p.DisplayOrder, cancellationToken);

            return (maxDisplayOrder ?? -1) + 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get next display order for user {UserId}", userId);
            throw new RepositoryException($"Failed to get next display order for user {userId}", ex);
        }
    }

    public async Task MarkAllAsSecondaryAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var photos = await _context.UserPhotos
                .Where(p => p.UserId == userId && p.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var photo in photos)
            {
                photo.SetAsSecondary();
            }

            if (photos.Any())
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Marked {Count} photos as secondary for user {UserId}",
                    photos.Count, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark all photos as secondary for user {UserId}", userId);
            throw new RepositoryException($"Failed to mark photos as secondary for user {userId}", ex);
        }
    }

    public async Task UpdatePrimaryPhotoAsync(
        Guid userId,
        Guid newPrimaryPhotoId,
        CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Mark all photos as secondary
            await MarkAllAsSecondaryAsync(userId, cancellationToken);

            // Set the new primary photo
            var newPrimaryPhoto = await GetByIdWithTrackingAsync(newPrimaryPhotoId, cancellationToken);
            if (newPrimaryPhoto != null && newPrimaryPhoto.UserId == userId)
            {
                newPrimaryPhoto.SetAsPrimary();
                await UpdateAsync(newPrimaryPhoto, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            _logger.LogDebug("Updated primary photo to {PhotoId} for user {UserId}",
                newPrimaryPhotoId, userId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to update primary photo for user {UserId}", userId);
            throw new RepositoryException($"Failed to update primary photo for user {userId}", ex);
        }
    }

    public async Task<UserPhoto?> GetPrimaryPhotoOrDefaultAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // First try to get the primary photo
            var primaryPhoto = await GetPrimaryPhotoAsync(userId, cancellationToken);

            // If no primary photo exists, get the first photo by display order
            if (primaryPhoto == null)
            {
                primaryPhoto = await _context.UserPhotos
                    .AsNoTracking()
                    .Where(p => p.UserId == userId)
                    .OrderBy(p => p.DisplayOrder)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return primaryPhoto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get primary or default photo for user {UserId}", userId);
            throw new RepositoryException($"Failed to retrieve primary or default photo for user {userId}", ex);
        }
    }

    public async Task<UserPhoto?> GetPhotoByDisplayOrderAsync(
        Guid userId,
        int displayOrder,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.UserPhotos
                .AsNoTracking()
                .Where(p => p.UserId == userId && p.DisplayOrder == displayOrder)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get photo by display order {DisplayOrder} for user {UserId}",
                displayOrder, userId);
            throw new RepositoryException($"Failed to retrieve photo by display order for user {userId}", ex);
        }
    }

    public async Task<bool> HasPhotosAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.UserPhotos
                .AnyAsync(p => p.UserId == userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if user {UserId} has photos", userId);
            throw new RepositoryException($"Failed to check if user {userId} has photos", ex);
        }
    }

    public async Task<bool> HasPrimaryPhotoAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.UserPhotos
                .AnyAsync(p => p.UserId == userId && p.IsPrimary, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if user {UserId} has primary photo", userId);
            throw new RepositoryException($"Failed to check if user {userId} has primary photo", ex);
        }
    }

    // Transaction support
    public async Task<IDbTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            return transaction.GetDbTransaction();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to begin transaction");
            throw new RepositoryException("Failed to begin transaction", ex);
        }
    }

    // Batch reordering
    public async Task ReorderPhotosAsync(
        Guid userId,
        Dictionary<Guid, int> photoOrderMap,
        CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var photos = await GetByUserIdWithTrackingAsync(userId, cancellationToken);

            foreach (var photo in photos)
            {
                if (photoOrderMap.TryGetValue(photo.Id, out var newOrder))
                {
                    var orderResult = photo.UpdateDisplayOrder(newOrder);
                    if (orderResult.IsFailure)
                    {
                        throw new RepositoryException($"Invalid display order {newOrder} for photo {photo.Id}");
                    }
                }
            }

            await UpdateRangeAsync(photos, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogDebug("Reordered {Count} photos for user {UserId}", photos.Count, userId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to reorder photos for user {UserId}", userId);
            throw new RepositoryException($"Failed to reorder photos for user {userId}", ex);
        }
    }
}

