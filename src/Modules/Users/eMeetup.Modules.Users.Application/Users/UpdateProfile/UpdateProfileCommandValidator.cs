using eMeetup.Modules.Users.Application.Users.UpdateUser;
using FluentValidation;

namespace eMeetup.Modules.Users.Application.Users.UpdateProfile;

internal sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    private static readonly string[] AllowedGenders = { "Male", "Female", "NonBinary", "PreferNotToSay" };

    public UpdateProfileCommandValidator()
    {
        // === Личная информация ===

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio cannot exceed 500 characters")
            .When(x => x.Bio is not null);

        RuleFor(x => x.DateOfBirth)
            .Must(BeAValidAge).WithMessage("User must be between 13 and 120 years old")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.Gender)
            .Must(g => AllowedGenders.Contains(g))
            .WithMessage($"Gender must be one of: {string.Join(", ", AllowedGenders)}")
            .When(x => !string.IsNullOrWhiteSpace(x.Gender));

        // === Контакты ===

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters")
            .Must(BeAValidPhone).WithMessage("Phone number format is invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Telegram)
            .MaximumLength(50).WithMessage("Telegram username cannot exceed 50 characters")
            .Matches("^@?[a-zA-Z0-9_]{5,32}$")
            .WithMessage("Telegram username format is invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.Telegram));

        RuleFor(x => x.Instagram)
            .MaximumLength(50).WithMessage("Instagram username cannot exceed 50 characters")
            .Matches("^@?[a-zA-Z0-9_.]{1,30}$")
            .WithMessage("Instagram username format is invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.Instagram));

        // === Местоположение ===

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters")
            .When(x => x.City is not null);

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters")
            .When(x => x.Country is not null);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90")
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180")
            .When(x => x.Longitude.HasValue);

        RuleFor(x => x.TimeZone)
            .MaximumLength(50).WithMessage("TimeZone cannot exceed 50 characters")
            .Must(BeAValidTimeZone).WithMessage("TimeZone is invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.TimeZone));

        // === Социальные характеристики ===

        RuleFor(x => x.Languages)
            .MaximumLength(100).WithMessage("Languages cannot exceed 100 characters")
            .Must(BeAValidLanguages).WithMessage("Languages must be comma-separated codes (e.g., 'ru,en,fr')")
            .When(x => !string.IsNullOrWhiteSpace(x.Languages));

        // === Интересы ===

        RuleFor(x => x.Interests)
            .MaximumLength(500).WithMessage("Interests cannot exceed 500 characters")
            .When(x => x.Interests is not null);
    }

    // ================================================================
    // === HELPERS ===
    // ================================================================

    private static bool BeAValidAge(DateTime? dateOfBirth)
    {
        if (!dateOfBirth.HasValue) return true;

        var today = DateTime.UtcNow;
        var age = today.Year - dateOfBirth.Value.Year;
        if (dateOfBirth.Value.Date > today.AddYears(-age)) age--;

        return age >= 13 && age <= 120;
    }

    private static bool BeAValidPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return true;

        var cleaned = new string(phone.Where(char.IsDigit).ToArray());
        return cleaned.Length >= 7 && cleaned.Length <= 15;
    }

    private static bool BeAValidTimeZone(string? timeZone)
    {
        if (string.IsNullOrWhiteSpace(timeZone)) return true;

        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool BeAValidLanguages(string? languages)
    {
        if (string.IsNullOrWhiteSpace(languages)) return true;

        var codes = languages.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return codes.All(c => c.Trim().Length >= 2);
    }
}
