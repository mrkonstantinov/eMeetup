using FluentValidation;

namespace eMeetup.Modules.Events.Application.MateTypes.CreateMateType;

internal sealed class CreateMateTypeCommandValidator : AbstractValidator<CreateMateTypeCommand>
{
    public CreateMateTypeCommandValidator()
    {
        // Basic required fields
        RuleFor(c => c.SessionId)
            .NotEmpty()
            .WithMessage("Event session ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Event session ID cannot be empty");

        RuleFor(c => c.Title)
            .NotEmpty()
            .WithMessage("Mate type title is required")
            .MaximumLength(100)
            .WithMessage("Mate type title cannot exceed 100 characters")
            .MinimumLength(3)
            .WithMessage("Mate type title must be at least 3 characters")
            .Matches(@"^[a-zA-Z0-9\s\-_]+$")
            .WithMessage("Mate type title can only contain letters, numbers, spaces, hyphens, and underscores");

        // Optional description validation
        RuleFor(c => c.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters")
            .When(c => !string.IsNullOrEmpty(c.Description));

        // Slot allocation validation
        RuleFor(c => c.AllocatedSlots)
            .GreaterThan(0)
            .WithMessage("Allocated slots must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Allocated slots cannot exceed 100");

        // Age range validation
        RuleFor(c => c)
            .Must(c => !c.MinAge.HasValue || !c.MaxAge.HasValue || c.MinAge <= c.MaxAge)
            .WithMessage("Minimum age cannot be greater than maximum age")
            .WithErrorCode("AgeRangeInvalid");

        RuleFor(c => c.MinAge)
            .GreaterThanOrEqualTo(18)
            .WithMessage("Minimum age cannot be less than 18")
            .LessThanOrEqualTo(120)
            .WithMessage("Minimum age cannot exceed 120")
            .When(c => c.MinAge.HasValue);

        RuleFor(c => c.MaxAge)
            .GreaterThanOrEqualTo(18)
            .WithMessage("Maximum age cannot be less than 18")
            .LessThanOrEqualTo(120)
            .WithMessage("Maximum age cannot exceed 120")
            .When(c => c.MaxAge.HasValue);

        // Age range reasonableness check
        RuleFor(c => c)
            .Must(c => !c.MinAge.HasValue || !c.MaxAge.HasValue || (c.MaxAge - c.MinAge) <= 50)
            .WithMessage("Age range cannot exceed 50 years")
            .WithErrorCode("AgeRangeTooWide");

        // Gender validation
        RuleFor(c => c.Gender)
            .IsInEnum()
            .WithMessage("Invalid gender value")
            .When(c => c.Gender.HasValue);

        RuleFor(c => c.PreferredGender)
            .IsInEnum()
            .WithMessage("Invalid preferred gender value")
            .When(c => c.PreferredGender.HasValue);

        // Preferred age range format validation
        RuleFor(c => c.PreferredAgeRange)
            .Matches(@"^(\d+-\d+|\d+\+|\d*)$")
            .WithMessage("Preferred age range must be in format '25-35', '35+', or empty")
            .MaximumLength(20)
            .WithMessage("Preferred age range cannot exceed 20 characters")
            .When(c => !string.IsNullOrEmpty(c.PreferredAgeRange));

        // Priority validation
        RuleFor(c => c.Priority)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Priority must be 0 or greater")
            .LessThanOrEqualTo(100)
            .WithMessage("Priority cannot exceed 100");

        // Cross-field validation rules
        RuleFor(c => c)
            .Must(c => !c.PreferredGender.HasValue || c.Gender.HasValue)
            .WithMessage("Cannot specify preferred gender without specifying participant gender")
            .WithErrorCode("PreferredGenderWithoutGender");

        RuleFor(c => c)
            .Must(c => !string.IsNullOrEmpty(c.PreferredAgeRange) || !c.PreferredGender.HasValue ||
                      (c.PreferredAgeRange != null && c.PreferredGender.HasValue))
            .WithMessage("If preferred gender is specified, preferred age range is recommended")
            .WithErrorCode("PreferredGenderWithoutAgeRange")
            .When(c => c.PreferredGender.HasValue);

        // Age consistency with preferred age range
        RuleFor(c => c)
            .Must(c => ValidateAgeRangeConsistency(c.MinAge, c.MaxAge, c.PreferredAgeRange))
            .WithMessage("Preferred age range should be compatible with the participant's age range")
            .WithErrorCode("InconsistentAgeRanges")
            .When(c => c.MinAge.HasValue && c.MaxAge.HasValue && !string.IsNullOrEmpty(c.PreferredAgeRange));
    }

    private bool ValidateAgeRangeConsistency(int? minAge, int? maxAge, string? preferredAgeRange)
    {
        if (!minAge.HasValue || !maxAge.HasValue || string.IsNullOrEmpty(preferredAgeRange))
            return true;

        // Parse preferred age range (e.g., "25-35" or "35+")
        if (preferredAgeRange.Contains('-'))
        {
            var parts = preferredAgeRange.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[0], out int prefMin) && int.TryParse(parts[1], out int prefMax))
            {
                // Check if preferred range overlaps with participant's age range
                return (prefMin <= maxAge && prefMax >= minAge);
            }
        }
        else if (preferredAgeRange.EndsWith('+'))
        {
            if (int.TryParse(preferredAgeRange.TrimEnd('+'), out int prefMin))
            {
                return prefMin <= maxAge;
            }
        }

        return true;
    }
}
