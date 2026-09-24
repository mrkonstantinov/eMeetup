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

namespace eMeetup.Modules.Users.Application.Users.Photos.Reorder;

internal sealed class ReorderPhotosCommandHandler(
    IUserRepository userRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    ILogger<ReorderPhotosCommandHandler> logger)
    : ICommandHandler<ReorderPhotosCommand>
{
    public async Task<Result> Handle(
        ReorderPhotosCommand request,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        try
        {
            logger.LogInformation(
                "Reordering {Count} photos for user {UserId}",
                request.OrderMap.Count,
                userId);

            var user = await userRepository.GetByIdWithPhotosAsync(userId, cancellationToken);
            if (user is null)
                return Result.Failure(UserErrors.NotFound(userId.ToString()));

            var result = user.ReorderPhotos(request.OrderMap);
            if (result.IsFailure)
                return result;

            var affectedRows = await unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
                return Result.Failure(UserErrors.DatabaseSaveFailed("No rows affected"));

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to reorder photos for user {UserId}", userId);
            return Result.Failure(PhotoErrors.InvalidOperation(ex.Message));
        }
    }
}
