using System;
using System.Collections.Generic;
using System.Text;

namespace eMeetup.Modules.Events.Domain.EventInvitations;

public enum ApprovalMode
{
    AutoApproval,    // Everyone matching criteria is auto-approved
    ManualOnly,      // All requests need manual approval
    Hybrid          // Some auto, some manual based on rules
}
