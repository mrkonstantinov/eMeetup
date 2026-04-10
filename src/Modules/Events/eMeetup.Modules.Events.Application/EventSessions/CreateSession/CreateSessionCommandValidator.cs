using eMeetup.Modules.Events.Application.MateTypes.CreateMateType;
using FluentValidation;

namespace eMeetup.Modules.Events.Application.EventSessions.CreateSession;

internal sealed class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionCommandValidator()
    {
        RuleFor(c => c.EventId).NotEmpty();
        RuleFor(c => c.Title).NotEmpty();
        RuleFor(c => c.Description).NotEmpty();
        RuleFor(c => c.StartsAtUtc).NotEmpty();
        RuleFor(c => c.EndsAtUtc).Must((cmd, endsAt) => endsAt > cmd.StartsAtUtc).When(c => c.EndsAtUtc.HasValue);
        RuleFor(c => c.Address).NotEmpty();
    }
}
