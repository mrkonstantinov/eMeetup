using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Application.Abstractions.Data;
using eMeetup.Modules.Users.Application.Abstractions.Identity;
using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Interfaces.Services;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Application.Users.RegisterUser;

internal sealed class RegisterUserCommandHandler(
    IIdentityProviderService identityProviderService,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<RegisterUserCommandHandler> logger)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        using var loggerScope = logger.BeginScope("UserRegistration {Email} {Username}",
            request.Email, request.Username);

        try
        {
            logger.LogInformation("Starting user registration process for {Email}", request.Email);

            // Check for existing user
            var existingUserResult = await CheckExistingUserAsync(request.Email, request.Username, cancellationToken);
            if (existingUserResult.IsFailure)
            {
                return Result.Failure<Guid>(existingUserResult.Error);
            }

            // Register with identity provider first
            logger.LogInformation("Registering user with identity provider: {Email}", request.Email);
            var identityResult = await identityProviderService.RegisterUserAsync(
                new UserModel(
                    request.Email,
                    request.Password,
                    request.Username,
                    request.DateOfBirth,
                    request.Gender),
                cancellationToken);

            if (identityResult.IsFailure)
            {
                logger.LogError("Identity provider registration failed for {Email}: {Error}",
                    request.Email, identityResult.Error);
                return Result.Failure<Guid>(identityResult.Error);
            }

            // Create user domain entity with basic information only
            logger.LogInformation("Creating user domain entity for {Email}", request.Email);
            var userResult = User.Create(
                request.Email,
                request.Username,
                request.DateOfBirth,
                request.Gender,
                identityResult.Value); // Only pass required fields

            if (userResult.IsFailure)
            {
                logger.LogError("User domain entity creation failed for {Email}: {Error}",
                    request.Email, userResult.Error);

                await TryCleanupIdentityUserAsync(identityResult.Value, request.Email);
                return Result.Failure<Guid>(userResult.Error);
            }

            var user = userResult.Value;

            // Save user to database
            logger.LogInformation("Saving user to database: {Email}", request.Email);
            userRepository.Insert(user);

            var affectedRows = await unitOfWork.SaveChangesAsync(cancellationToken);

            if (affectedRows == 0)
            {
                logger.LogError("No rows were affected during database save for {Email}", request.Email);
                await TryCleanupIdentityUserAsync(identityResult.Value, request.Email);
                return Result.Failure<Guid>(UserErrors.DatabaseSaveFailed);
            }

            logger.LogInformation("User registered successfully: {Email} with ID: {UserId}",
                request.Email, user.Id);

            return user.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred during user registration for {Email}",
                request.Email);
            return Result.Failure<Guid>(UserErrors.RegistrationFailed);
        }
    }

    private async Task<Result> CheckExistingUserAsync(string email, string username, CancellationToken cancellationToken)
    {
        var existingByEmail = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (existingByEmail != null)
        {
            logger.LogWarning("Registration attempt with existing email: {Email}", email);
            return Result.Failure(UserErrors.EmailAlreadyExists(email));
        }

        var existingByUsername = await userRepository.GetByUsernameAsync(username, cancellationToken);
        if (existingByUsername != null)
        {
            logger.LogWarning("Registration attempt with existing username: {Username}", username);
            return Result.Failure(UserErrors.UsernameAlreadyExists(username));
        }

        return Result.Success();
    }

    private async Task TryCleanupIdentityUserAsync(string identityId, string email)
    {
        try
        {
            logger.LogInformation("Attempting to cleanup identity user: {Email} (IdentityId: {IdentityId})",
                email, identityId);

            // Implementation depends on your identity service
            logger.LogWarning("Identity user cleanup not implemented for: {Email}", email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during identity user cleanup for: {Email}", email);
        }
    }
}
