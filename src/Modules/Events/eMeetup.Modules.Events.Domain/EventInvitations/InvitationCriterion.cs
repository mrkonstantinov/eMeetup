using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace eMeetup.Modules.Events.Domain.EventInvitations;

public class InvitationCriterion
{
    public Guid Id { get; set; }
    public Guid InvitationId { get; set; }
    public EventInvitation Invitation { get; set; }

    // Criterion type
    public CriterionType Type { get; set; } // Age, Gender, Occupation, etc.

    // For simple values (Gender, Occupation, etc.)
    public string StringValue { get; set; }

    // For range values (Age, Experience, etc.)
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }

    // For boolean criteria
    public bool? BoolValue { get; set; }

    // For list-based criteria (multiple allowed values)
    public string AllowedValuesJson { get; set; } // ["Student", "Engineer"]

    // For complex criteria (using JSON for flexibility)
    public string ComplexCriteriaJson { get; set; }

    // Helper method to evaluate if a user matches
    public bool Matches(UserDemographics user)
    {
        return Type switch
        {
            CriterionType.Age => EvaluateAge(user.Age),
            CriterionType.Gender => EvaluateGender(user.Gender),
            CriterionType.Occupation => EvaluateOccupation(user.Occupation),
            CriterionType.Location => EvaluateLocation(user.City, user.Country),
            CriterionType.Experience => EvaluateExperience(user.YearsOfExperience),
            CriterionType.Interest => EvaluateInterest(user.Interests),
            CriterionType.Custom => EvaluateCustom(user),
            _ => false
        };
    }

    private bool EvaluateAge(int? age)
    {
        if (!age.HasValue) return false;

        if (MinValue.HasValue && age < MinValue.Value) return false;
        if (MaxValue.HasValue && age > MaxValue.Value) return false;

        return true;
    }

    private bool EvaluateGender(string gender)
    {
        if (string.IsNullOrEmpty(StringValue)) return true;
        return string.Equals(gender, StringValue, StringComparison.OrdinalIgnoreCase);
    }

    private bool EvaluateOccupation(string occupation)
    {
        if (string.IsNullOrEmpty(AllowedValuesJson)) return true;

        var allowed = JsonSerializer.Deserialize<List<string>>(AllowedValuesJson);
        return allowed.Contains(occupation);
    }

    private bool EvaluateLocation(string city, string country)
    {
        // Complex logic for location matching
        return true; // Simplified
    }

    private bool EvaluateExperience(int? years)
    {
        if (!years.HasValue) return false;

        if (MinValue.HasValue && years < MinValue.Value) return false;
        if (MaxValue.HasValue && years > MaxValue.Value) return false;

        return true;
    }

    private bool EvaluateInterest(List<string> interests)
    {
        if (string.IsNullOrEmpty(AllowedValuesJson)) return true;

        var required = JsonSerializer.Deserialize<List<string>>(AllowedValuesJson);
        return required.Any(r => interests.Contains(r));
    }

    private bool EvaluateCustom(UserDemographics user)
    {
        // For extremely complex criteria, evaluate the stored JSON expression
        if (string.IsNullOrEmpty(ComplexCriteriaJson)) return true;

        // Use a rules engine or dynamic LINQ here
        return true; // Simplified
    }
}
