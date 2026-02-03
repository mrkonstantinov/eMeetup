using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Users;
using eMeetup.Modules.Users.Infrastructure.Database;
using eMeetup.Modules.Users.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Infrastructure.Photos;

public class PhotoRepository(UsersDbContext context, ILogger<PhotoRepository> logger) : IPhotoRepository
{
    private readonly UsersDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogger<PhotoRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result> AddPhotoAsync(Guid userId, string url, bool isPrimary = false, CancellationToken cancellationToken = default)
    {
        try
        {
            // First, check if user exists and get current photo count
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);
            if (!userExists)
                return Result.Failure(UserErrors.NotFound(userId.ToString()));

            var photoCount = await _context.UserPhotos
                .CountAsync(p => p.UserId == userId, cancellationToken);

            if (photoCount >= 10)
                return Result.Failure(UserErrors.TooManyPhotos(10));

            var displayOrder = photoCount; // Next available order

            var photoResult = UserPhoto.Create(userId, url, displayOrder, isPrimary);
            if (photoResult.IsFailure)
                return Result.Failure(photoResult.Error);

            var photo = photoResult.Value;
            _context.UserPhotos.Add(photo);

            if (isPrimary)
            {
                // Clear other primary photos
                await _context.UserPhotos
                    .Where(p => p.UserId == userId && p.Id != photo.Id)
                    .ExecuteUpdateAsync(setters =>
                        setters.SetProperty(p => p.IsPrimary, false), cancellationToken);

                // Update user's profile picture
                await _context.Users
                    .Where(u => u.Id == userId)
                    .ExecuteUpdateAsync(setters =>
                        setters.SetProperty(u => u.ProfilePictureUrl, url)
                               .SetProperty(u => u.UpdatedAt, DateTime.UtcNow), cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Photo.AddFailed", ex.Message, ErrorType.Failure));
        }
    }

    public async Task<Result> AddPhotosAsync(Guid userId, IEnumerable<string> urls, CancellationToken cancellationToken = default)
    {
        var urlsList = urls.ToList();
        if (!urlsList.Any())
            return Result.Success();

        try
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);
            if (!userExists)
                return Result.Failure(UserErrors.NotFound(userId.ToString()));

            var currentCount = await _context.UserPhotos
                .CountAsync(p => p.UserId == userId, cancellationToken);

            if (currentCount + urlsList.Count > 10)
                return Result.Failure(UserErrors.TooManyPhotos(10));

            var photos = new List<UserPhoto>();
            for (int i = 0; i < urlsList.Count; i++)
            {
                var photoResult = UserPhoto.Create(userId, urlsList[i], currentCount + i);
                if (photoResult.IsFailure)
                    return Result.Failure(photoResult.Error);

                photos.Add(photoResult.Value);
            }

            _context.UserPhotos.AddRange(photos);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Photos.AddFailed", ex.Message, ErrorType.Failure));
        }
    }

    public async Task<Result> SetPrimaryPhotoAsync(Guid userId, Guid photoId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get the photo
            var photo = await _context.UserPhotos
                .FirstOrDefaultAsync(p => p.Id == photoId && p.UserId == userId, cancellationToken);

            if (photo == null)
                return Result.Failure(UserErrors.PhotoNotFound(photoId));

            // Clear other primary photos
            await _context.UserPhotos
                .Where(p => p.UserId == userId && p.Id != photoId)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(p => p.IsPrimary, false), cancellationToken);

            // Set this photo as primary
            await _context.UserPhotos
                .Where(p => p.Id == photoId)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(p => p.IsPrimary, true), cancellationToken);

            // Update user's profile picture
            await _context.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(u => u.ProfilePictureUrl, photo.Url)
                           .SetProperty(u => u.UpdatedAt, DateTime.UtcNow), cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Photo.SetPrimaryFailed", ex.Message, ErrorType.Failure));
        }
    }

    public async Task<Result> RemovePhotoAsync(Guid userId, Guid photoId, CancellationToken cancellationToken = default)
    {
        try
        {
            var photo = await _context.UserPhotos
                .FirstOrDefaultAsync(p => p.Id == photoId && p.UserId == userId, cancellationToken);

            if (photo == null)
                return Result.Failure(UserErrors.PhotoNotFound(photoId));

            var isPrimary = photo.IsPrimary;

            // Remove the photo
            _context.UserPhotos.Remove(photo);
            await _context.SaveChangesAsync(cancellationToken);

            if (isPrimary)
            {
                // Find a new primary photo
                var newPrimary = await _context.UserPhotos
                    .Where(p => p.UserId == userId)
                    .OrderBy(p => p.DisplayOrder)
                    .FirstOrDefaultAsync(cancellationToken);

                if (newPrimary != null)
                {
                    await _context.UserPhotos
                        .Where(p => p.Id == newPrimary.Id)
                        .ExecuteUpdateAsync(setters =>
                            setters.SetProperty(p => p.IsPrimary, true), cancellationToken);

                    await _context.Users
                        .Where(u => u.Id == userId)
                        .ExecuteUpdateAsync(setters =>
                            setters.SetProperty(u => u.ProfilePictureUrl, newPrimary.Url), cancellationToken);
                }
                else
                {
                    await _context.Users
                        .Where(u => u.Id == userId)
                        .ExecuteUpdateAsync(setters =>
                            setters.SetProperty(u => u.ProfilePictureUrl, (string?)null), cancellationToken);
                }
            }

            // Reorder remaining photos
            await ReorderUserPhotosAsync(userId, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Photo.RemoveFailed", ex.Message, ErrorType.Failure));
        }
    }

    public async Task<Result> ReorderPhotosAsync(Guid userId, Dictionary<Guid, int> photoOrders, CancellationToken cancellationToken = default)
    {
        // Validate no duplicate orders
        var duplicateOrders = photoOrders.Values
            .GroupBy(x => x)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateOrders.Any())
            return Result.Failure(UserErrors.DuplicateDisplayOrder);

        // Validate all orders are non-negative
        if (photoOrders.Values.Any(order => order < 0))
            return Result.Failure(UserErrors.InvalidDisplayOrder);

        try
        {
            // Update each photo's order
            foreach (var (photoId, newOrder) in photoOrders)
            {
                await _context.UserPhotos
                    .Where(p => p.Id == photoId && p.UserId == userId)
                    .ExecuteUpdateAsync(setters =>
                        setters.SetProperty(p => p.DisplayOrder, newOrder), cancellationToken);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Photo.ReorderFailed", ex.Message, ErrorType.Failure));
        }
    }

    public async Task<List<UserPhoto>> GetUserPhotosAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserPhotos
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserPhoto?> GetPhotoAsync(Guid photoId, CancellationToken cancellationToken = default)
    {
        return await _context.UserPhotos
            .FirstOrDefaultAsync(p => p.Id == photoId, cancellationToken);
    }

    private async Task ReorderUserPhotosAsync(Guid userId, CancellationToken cancellationToken)
    {
        var photos = await _context.UserPhotos
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);

        for (int i = 0; i < photos.Count; i++)
        {
            await _context.UserPhotos
                .Where(p => p.Id == photos[i].Id)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(p => p.DisplayOrder, i), cancellationToken);
        }
    }
}
