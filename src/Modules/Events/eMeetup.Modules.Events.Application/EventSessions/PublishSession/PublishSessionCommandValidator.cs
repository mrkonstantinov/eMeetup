using FluentValidation;

namespace eMeetup.Modules.Events.Application.EventSessions.PublishSession;

internal sealed class PublishSessionCommandValidator : AbstractValidator<PublishSessionCommand>
{
    public PublishSessionCommandValidator()
    {
        RuleFor(c => c.SessionId).NotEmpty();
    }
}
