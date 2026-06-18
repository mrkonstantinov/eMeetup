using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Events.Application.Participants.UpdateParticipant;

public sealed record UpdateParticipantCommand(Guid ParticipantId, string UserName, DateTime SyncedAt) : ICommand;
