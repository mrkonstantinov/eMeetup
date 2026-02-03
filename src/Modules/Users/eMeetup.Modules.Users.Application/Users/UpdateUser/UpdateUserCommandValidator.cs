using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Modules.Users.Application.Users.RegisterUser;
using FluentValidation;

namespace eMeetup.Modules.Users.Application.Users.UpdateUser
{
    internal sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {

        private static readonly string[] AllowedImageTypes =
        {
        "image/jpeg", "image/png", "image/gif", "image/webp"
        };

        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public UpdateUserCommandValidator()
        {
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

        private static bool BeValidImageType(string contentType)
        {
            return AllowedImageTypes.Contains(contentType?.ToLower());
        }

        private static bool HaveBothCoordinatesOrNone(UpdateUserCommand command)
        {
            return (command.Latitude.HasValue && command.Longitude.HasValue) ||
                   (!command.Latitude.HasValue && !command.Longitude.HasValue);
        }

        private static bool HaveCityAndCountryIfCoordinatesProvided(UpdateUserCommand command)
        {
            if (!command.Latitude.HasValue || !command.Longitude.HasValue)
                return true;

            return !string.IsNullOrWhiteSpace(command.City) &&
                   !string.IsNullOrWhiteSpace(command.Country);
        }
    }
}
