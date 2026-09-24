using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Application.Abstractions;
using eMeetup.Modules.Users.Application.Abstractions.Authentication;
using eMeetup.Modules.Users.Application.Abstractions.Data;
using eMeetup.Modules.Users.Domain.Errors;
using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Photos;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Application.Users.Photos.SetPrimary;

internal sealed class SetPrimaryPhotoCommandHandler(
    IUserRepository userRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    ILogger<SetPrimaryPhotoCommandHandler> logger)
    : ICommandHandler<SetPrimaryPhotoCommand>
{
    public async Task<Result> Handle(
        SetPrimaryPhotoCommand request,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        using var loggerScope = logger.BeginScope(
            "SetPrimaryPhoto {UserId} {PhotoId}",
            userId,
            request.PhotoId);

        try
        {
            logger.LogInformation(
                "Setting primary photo {PhotoId} for user {UserId}",
                request.PhotoId,
                userId);

            var user = await userRepository.GetByIdWithPhotosAsync(userId, cancellationToken);

            if (user is null)
                return Result.Failure(UserErrors.NotFound(userId.ToString()));

            var result = user.SetPrimaryPhoto(request.PhotoId);
            if (result.IsFailure)
                return result;

            var affectedRows = await unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
                return Result.Failure(UserErrors.DatabaseSaveFailed("No rows affected"));

            logger.LogInformation(
                "Primary photo {PhotoId} set successfully for user {UserId}",
                request.PhotoId,
                userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to set primary photo {PhotoId} for user {UserId}",
                request.PhotoId,
                userId);

            return Result.Failure(PhotoErrors.InvalidOperation(ex.Message));
        }
    }
}
