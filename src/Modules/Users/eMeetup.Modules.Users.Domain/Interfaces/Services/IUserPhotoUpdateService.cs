using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.AspNetCore.Http;

namespace eMeetup.Modules.Users.Domain.Interfaces.Services;

public interface IUserPhotoUpdateService
{
    Task<PhotoUpdateResult> UpdateUserPhotosAsync(Guid userId, List<IFormFile>? photos, CancellationToken cancellationToken = default);
    Task<PhotoUpdateResult> ReplaceUserPhotoAsync(ReplaceUserPhotoCommand command, CancellationToken cancellationToken = default);
    Task<PrimaryPhotoInfo?> GetPrimaryPhotoInfoAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<string?> GetPrimaryPhotoUrlAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PhotoValidationResult> ValidatePhotosAsync(List<IFormFile> photos, CancellationToken cancellationToken = default);
    Task<PhotoSummary> GetPhotoSummaryAsync(Guid userId, CancellationToken cancellationToken = default);
}

// Supporting DTOs and Results
public class UpdateUserPhotosCommand
{
    public Guid UserId { get; set; }
    public List<IFormFile>? Photos { get; set; }
}

public class ReplaceUserPhotoCommand
{
    public Guid UserId { get; set; }
    public IFormFile? NewPhoto { get; set; }
}

public class PhotoUpdateResult
{
    public Guid UserId { get; set; }
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<UserPhoto> UpdatedPhotos { get; set; } = new();
    public List<UserPhoto> CreatedPhotos { get; set; } = new();
    public List<UserPhoto> DeletedPhotos { get; set; } = new();
    public List<string> DeletedPhotoUrls { get; set; } = new();
    public List<string> UploadedFileUrls { get; set; } = new();
    public UserPhoto? NewPrimaryPhoto { get; set; }
    public string? NewPrimaryPhotoUrl { get; set; }
}

public class PhotoValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<IFormFile> ValidFiles { get; set; } = new();
}

public class FileValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ValidationResult
{
    public bool IsValid => !Errors.Any();
    public List<string> Errors { get; set; } = new();
}

public class PrimaryPhotoInfo
{
    public Guid PhotoId { get; set; }
    public string Url { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime UploadedAt { get; set; }
    public Guid UserId { get; set; }
}

public class PhotoSummary
{
    public Guid UserId { get; set; }
    public int TotalCount { get; set; }
    public bool HasPhotos { get; set; }
    public bool HasPrimaryPhoto { get; set; }
    public string? PrimaryPhotoUrl { get; set; }
    public Guid? PrimaryPhotoId { get; set; }
    public List<PhotoItem> Photos { get; set; } = new();
}

public class PhotoItem
{
    public Guid Id { get; set; }
    public string Url { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime UploadedAt { get; set; }
}
