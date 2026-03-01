using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Modules.Events.Domain.EventInvitations;
using eMeetup.Modules.Events.Domain.Events;

namespace eMeetup.Modules.Events.Domain.Registrations;

public class AttendeeRegistration
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Event Event { get; set; }

    // Which invitation they used (if any)
    public Guid? InvitationId { get; set; }
    public EventInvitation Invitation { get; set; }

    // User snapshot at registration time
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public string UserDisplayName { get; set; }

    // Demographics at registration (for historical accuracy)
    public int? AgeAtRegistration { get; set; }
    public string Gender { get; set; }
    public string Occupation { get; set; }
    public string Location { get; set; }

    // Registration details
    public DateTime RegisteredAt { get; set; }
    public RegistrationStatus Status { get; set; }
    public string RegistrationCode { get; set; } // If using invitation codes

    // Check-in
    public DateTime? CheckedInAt { get; set; }
    public string CheckedInBy { get; set; }

    // Additional data
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public enum RegistrationStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Attended,
    NoShow
}
