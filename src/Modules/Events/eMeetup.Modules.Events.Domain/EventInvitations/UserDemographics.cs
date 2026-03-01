using System;
using System.Collections.Generic;
using System.Text;

namespace eMeetup.Modules.Events.Domain.EventInvitations;

public class UserDemographics
{
    public Guid UserId { get; set; }
    public int? Age { get; set; }
    public string Gender { get; set; }
    public string Occupation { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public int? YearsOfExperience { get; set; }
    public List<string> Interests { get; set; } = new();
    public int EventsAttended { get; set; }
    public bool IsEmailVerified { get; internal set; }
    public int TrustScore { get; internal set; }
}
