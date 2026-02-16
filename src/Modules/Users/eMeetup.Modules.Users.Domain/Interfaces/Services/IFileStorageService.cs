using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Photos;
using Microsoft.AspNetCore.Http;

namespace eMeetup.Modules.Users.Domain.Interfaces.Services;
public interface IFileStorageService
{
    Task<Result<FileUploadResult>> UploadFileAsync(IFormFile file, CancellationToken cancellationToken);
    Task<Result> DeleteFileAsync(string url, CancellationToken cancellationToken);
}
