using FluentValidation;

namespace eMeetup.Modules.Events.Application.Events.CreateEvent;

internal class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(c => c.Title).NotEmpty();
        RuleFor(c => c.Description).NotEmpty();
        //RuleFor(c => c.StartsAtUtc).NotEmpty();
        //RuleFor(c => c.EndsAtUtc).Must((cmd, endsAt) => endsAt > cmd.StartsAtUtc).When(c => c.EndsAtUtc.HasValue);
    }
}
