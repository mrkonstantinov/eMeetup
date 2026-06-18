using FluentValidation;

namespace eMeetup.Modules.Events.Application.Participants.UpdateParticipant;

internal sealed class UpdateParticipantCommandValidator : AbstractValidator<UpdateParticipantCommand>
{
    public UpdateParticipantCommandValidator()
    {
        RuleFor(c => c.ParticipantId).NotEmpty();
        RuleFor(c => c.UserName).NotEmpty();
    }
}
