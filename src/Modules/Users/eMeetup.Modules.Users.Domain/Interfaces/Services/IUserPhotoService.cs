using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.AspNetCore.Http;

namespace eMeetup.Modules.Users.Domain.Interfaces.Services;

public interface IUserPhotoService
{
    Task<Result<List<string>>> UploadUserPhotosAsync(
            Guid userId,
            List<IFormFile> photos,
            CancellationToken cancellationToken = default);

    Task<Result> DeleteUserPhotosAsync(
        Guid userId,
        List<string> photoUrls,
        CancellationToken cancellationToken = default);

    Task<Result<string?>> ProcessPhotosAndUpdateProfileAsync(
        Guid userId,
        List<IFormFile>? photos,
        CancellationToken cancellationToken = default);
}
