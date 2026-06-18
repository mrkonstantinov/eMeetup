using FluentValidation;

namespace eMeetup.Modules.Events.Application.Participants.CreateParticipant;

internal sealed class CreateParticipantCommandValidator : AbstractValidator<CreateParticipantCommand>
{
    public CreateParticipantCommandValidator()
    {
        RuleFor(c => c.ParticipantId).NotEmpty();
        RuleFor(c => c.Email).EmailAddress();
        RuleFor(c => c.UserName).NotEmpty();
        RuleFor(c => c.DateOfBirth).NotEmpty();
        RuleFor(c => c.Gender).NotEmpty();
    }
}
