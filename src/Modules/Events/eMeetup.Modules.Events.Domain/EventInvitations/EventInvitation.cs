using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Modules.Events.Domain.Events;

namespace eMeetup.Modules.Events.Domain.EventInvitations;

public class EventInvitation
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Event Event { get; set; }

    // Basic info
    public string Name { get; set; }
    public string Description { get; set; }


    // Capacity
    public int? MaxAttendees { get; set; }
    public int CurrentRegistrations { get; set; }
    public int PendingApprovals { get; set; }

    // Approval configuration
    public ApprovalMode ApprovalMode { get; set; }
    public AutoApprovalRules AutoApprovalRules { get; set; } // JSON
    public int? MaxPendingPerUser { get; set; } // Prevent spam

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
