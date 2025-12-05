using eMeetup.Modules.Users.Domain.Users;
using FluentValidation;

namespace eMeetup.Modules.Users.Application.Users.RegisterUser;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    private static readonly string[] AllowedImageTypes =
    {
        "image/jpeg", "image/png", "image/gif", "image/webp"
    };

    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("A valid email address is required")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long")
            .MaximumLength(50).WithMessage("Username cannot exceed 50 characters")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.UtcNow.Date).WithMessage("Date of birth must be in the past")
            .Must(BeAtLeast18YearsOld).WithMessage("You must be at least 18 years old to register");

        // Simplified gender validation since it's now an enum
        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Invalid gender value");

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Bio));

        // Location validation
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90")
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180")
            .When(x => x.Longitude.HasValue);

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Country));

        // Photo validation
        RuleFor(x => x.Photos)
            .Must(photos => photos == null || photos.Count <= 10)
            .WithMessage("Maximum 10 photos allowed");

        RuleForEach(x => x.Photos)
            .ChildRules(photo =>
            {
                photo.RuleFor(f => f.Length)
                    .LessThanOrEqualTo(MaxFileSize)
                    .WithMessage("Photo size cannot exceed 5MB")
                    .When(f => f != null);

                photo.RuleFor(f => f.ContentType)
                    .Must(BeValidImageType)
                    .WithMessage("Photo must be a valid image type (JPEG, PNG, GIF, WEBP)")
                    .When(f => f != null);
            })
            .When(x => x.Photos != null);

        // Cross-property validation
        RuleFor(x => x)
            .Must(HaveBothCoordinatesOrNone)
            .WithMessage("Both latitude and longitude must be provided together")
            .OverridePropertyName("Location");

        RuleFor(x => x)
            .Must(HaveCityAndCountryIfCoordinatesProvided)
            .WithMessage("City and country are recommended when providing coordinates")
            .When(x => x.Latitude.HasValue && x.Longitude.HasValue);
    }

    private static bool BeAtLeast18YearsOld(DateTime dateOfBirth)
    {
        var age = DateTime.Today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > DateTime.Today.AddYears(-age))
            age--;
        return age >= 18;
    }

    private static bool BeValidImageType(string contentType)
    {
        return AllowedImageTypes.Contains(contentType?.ToLower());
    }

    private static bool HaveBothCoordinatesOrNone(RegisterUserCommand command)
    {
        return (command.Latitude.HasValue && command.Longitude.HasValue) ||
               (!command.Latitude.HasValue && !command.Longitude.HasValue);
    }

    private static bool HaveCityAndCountryIfCoordinatesProvided(RegisterUserCommand command)
    {
        if (!command.Latitude.HasValue || !command.Longitude.HasValue)
            return true;

        return !string.IsNullOrWhiteSpace(command.City) &&
               !string.IsNullOrWhiteSpace(command.Country);
    }
}
