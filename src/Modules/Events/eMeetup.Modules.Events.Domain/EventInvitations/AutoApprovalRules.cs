using System;
using System.Collections.Generic;
using System.Text;

namespace eMeetup.Modules.Events.Domain.EventInvitations;

public class AutoApprovalRules
{
    public bool AutoApproveVerifiedUsers { get; set; }
    public int? MinTrustScore { get; set; }
    //public List<string> AutoApproveRoles { get; set; }
    public int? PreviousEventsAttended { get; set; }
    public bool AutoApproveFirstTime { get; set; }
    public TimeSpan? AutoApproveWithin { get; set; } // Auto-approve if requested X days before event

    public bool Evaluate(UserDemographics user)
    {
        if (AutoApproveVerifiedUsers && user.IsEmailVerified)
            return true;

        if (MinTrustScore.HasValue && user.TrustScore >= MinTrustScore.Value)
            return true;

        //if (AutoApproveRoles != null && AutoApproveRoles.Any(r => user.Roles.Contains(r)))
        //    return true;

        if (PreviousEventsAttended.HasValue && user.EventsAttended >= PreviousEventsAttended.Value)
            return true;

        if (AutoApproveFirstTime && user.EventsAttended == 0)
            return true;

        if (AutoApproveWithin.HasValue)
        {
            // Auto-approve if requested close to event
            // Implementation depends on your logic
        }

        return false;
    }
}
