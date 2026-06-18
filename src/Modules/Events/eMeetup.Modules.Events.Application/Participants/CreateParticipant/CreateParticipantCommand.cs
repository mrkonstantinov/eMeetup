using eMeetup.Common.Application.Messaging;
using eMeetup.Modules.Events.Domain.Participants;

namespace eMeetup.Modules.Events.Application.Participants.CreateParticipant;

public sealed record CreateParticipantCommand(Guid ParticipantId, string Email, string UserName, DateTime DateOfBirth, Gender Gender, DateTime SyncedAt)
    : ICommand;
