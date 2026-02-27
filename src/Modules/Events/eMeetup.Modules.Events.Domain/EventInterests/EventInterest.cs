using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Domain;
using eMeetup.Modules.Events.Domain.Events;
using eMeetup.Modules.Events.Domain.Tags;

namespace eMeetup.Modules.Events.Domain.EventInterests
{
    public class EventInterest
    {
        public Guid Id { get; private set; }
        public Guid EventId { get; private set; }
        public Guid TagId { get; private set; }

        private EventInterest(Guid eventId, Guid tagId)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            TagId = tagId;
        }

        // Factory method
        public static Result<EventInterest> Create(Guid eventId, Guid tagId)
        {
            // Validation

            var userInterest = new EventInterest(eventId, tagId);
            return Result.Success(userInterest);
        }


        // Navigation properties
        public virtual Event Event { get; private set; } = null!;
        public virtual Tag Tag { get; private set; } = null!;
    }
}
