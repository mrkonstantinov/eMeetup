using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Events.Application.Abstractions.Data;
using eMeetup.Modules.Events.Domain.Events;
using eMeetup.Modules.Events.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Events.Application.Events.CreateEvent;

internal sealed class CreateEventCommandHandler(
    //IDateTimeProvider dateTimeProvider,
    IEventRepository eventRepository,
    IEventTagsRepository eventTagsRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateEventCommandHandler> logger)
    : ICommandHandler<CreateEventCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        //if (request.StartsAtUtc < dateTimeProvider.UtcNow)
        //{
        //    return Result.Failure<Guid>(EventErrors.StartDateInPast);
        //}

        Result<Event> result = Event.Create(
            request.CreatorId,
            request.Title,
            request.Description,
            request.Url,
            request.Tags);

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        eventRepository.Insert(result.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var tagsResult = await HandleEventTagsAsync(result.Value, request.Tags, cancellationToken);
        if (tagsResult.IsFailure)
        {
            logger.LogError("");
        };

        return result.Value.Id;
    }

    private async Task<Result> HandleEventTagsAsync(
        Event @event,
        string? tags,
        CancellationToken cancellationToken)
    {
        // Get current interests
        var currentTags = await eventTagsRepository.GetByEventIdAsync(@event.Id, cancellationToken);
        var currentTagsSet = new HashSet<string>(
            currentTags.Select(ui => ui.Tag.Name.Trim()),
            StringComparer.OrdinalIgnoreCase);

        // Parse requested interests
        var requestedTagsSet = ParseInterestNames(tags);

        // Check if interests changed (order doesn't matter)
        if (!AreInterestSetsEqual(currentTagsSet, requestedTagsSet))
        {
            // Update user interests
            var updatedTags = await eventTagsRepository.UpdateEventTagsAsync(
                @event.Id,
                tags ?? string.Empty,
                cancellationToken);
        }

        return Result.Success();
    }

    private HashSet<string> ParseInterestNames(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(i => i.Trim())
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private bool AreInterestSetsEqual(HashSet<string>? set1, HashSet<string>? set2)
    {
        // If both are null or empty
        if ((set1 == null || set1.Count == 0) && (set2 == null || set2.Count == 0))
            return true;

        // If one is null/empty and the other has items
        if (set1 == null || set2 == null)
            return false;

        return set1.Count == set2.Count && set1.SetEquals(set2);
    }
}

