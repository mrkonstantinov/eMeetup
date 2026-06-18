using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Domain;

namespace eMeetup.Modules.Events.Domain.EventSessions;

public static class EventSessionErrors
{
    public static Error NotFound(Guid sessionId) =>
        Error.NotFound("EventSession.NotFound", $"The session with the identifier {sessionId} was not found");

    public static readonly Error InvalidEventId = Error.Problem(
        "Events.InvalidEventId", "Event ID is required");

    public static readonly Error InvalidTitle = Error.Problem(
        "Events.InvalidTitle", "Event session title is required");

    public static readonly Error TitleTooLong = Error.Problem(
        "Events.TitleTooLong", "Event session title cannot exceed 200 characters");

    public static readonly Error InvalidLocality = Error.Problem(
        "Events.InvalidLocality", "Locality is required");

    public static readonly Error InvalidAddress = Error.Problem(
        "Events.InvalidAddress", "Address is required");

    public static readonly Error StartDateInPast = Error.Problem(
        "Events.StartDateInPast", "Event start date cannot be in the past");

    public static readonly Error EndDatePrecedesStartDate = Error.Problem(
        "Events.EndDatePrecedesStartDate", "Event end date must be after start date");

    public static readonly Error CannotModifyPublishedSession = Error.Problem(
        "Events.CannotModifyPublishedSession", "Cannot modify a published session");

    public static readonly Error CannotPublishPastEvent = Error.Problem(
        "Events.CannotPublishPastEvent", "Cannot publish an event that starts in the past");

    public static readonly Error CannotCancelCompletedSession = Error.Problem(
        "Events.CannotCancelCompletedSession", "Cannot cancel a completed session");

    public static readonly Error CannotCompleteUnpublishedSession = Error.Problem(
        "Events.CannotCompleteUnpublishedSession", "Cannot complete an unpublished session");

    public static readonly Error CannotCompleteFutureEvent = Error.Problem(
        "Events.CannotCompleteFutureEvent", "Cannot complete a future event");

    public static readonly Error SessionNotAvailable = Error.Problem(
        "Events.SessionNotAvailable", "Session is not available for registration");

    public static readonly Error SessionAlreadyStarted = Error.Problem(
        "Events.SessionAlreadyStarted", "Session has already started");

    public static readonly Error UserNotEligible = Error.Problem(
        "Events.UserNotEligible", "User does not meet eligibility requirements");

    public static readonly Error SessionFull = Error.Problem(
        "Events.SessionFull", "Session is full");

    public static readonly Error InvalidStatusTransition = Error.Problem(
        "Events.InvalidStatusTransition", "Invalid status transition");

    public static readonly Error MateTypeNotFound = Error.NotFound(
        "Events.MateTypeNotFound", "Mate type not found");
}
