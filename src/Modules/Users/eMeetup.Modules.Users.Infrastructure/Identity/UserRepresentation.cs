using System.Globalization;
using eMeetup.Modules.Users.Application.Abstractions.Identity;
using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Infrastructure.Identity;

internal sealed record UserRepresentation(
    string Username,
    string Email,
    Dictionary<string, List<string>> Attributes,
    bool EmailVerified,
    bool Enabled,
    CredentialRepresentation[] Credentials);


internal sealed record UserProfileRepresentation(
    string? Username,
    string? Email,
    Dictionary<string, List<string>> Attributes,
    bool? EmailVerified,
    bool? Enabled
    //List<CredentialRepresentation>? Credentials = null
    )
{
    // Helper properties to easily access attributes (optional but convenient)
    //public Gender? Gender => GetSingleAttributeValue("gender") is string genderStr
    //    && Enum.TryParse<Gender>(genderStr, true, out var gender) ? gender : null;

    //public DateTime? DateOfBirth => GetSingleAttributeValue("dateOfBirth") is string dobStr
    //    && DateTime.TryParse(dobStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob) ? dob : null;

    //public string? Bio => GetSingleAttributeValue("bio");
    //public string? ProfilePictureUrl => GetSingleAttributeValue("profilePictureUrl");
    //public string? City => GetSingleAttributeValue("city");
    //public string? Country => GetSingleAttributeValue("country");
    //public string? Interests => GetSingleAttributeValue("interests");

    //public double? Latitude => GetSingleAttributeValue("latitude") is string latStr
    //    && double.TryParse(latStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var lat) ? lat : null;

    //public double? Longitude => GetSingleAttributeValue("longitude") is string lonStr
    //    && double.TryParse(lonStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var lon) ? lon : null;

    // Helper method to get single attribute value
    private string? GetSingleAttributeValue(string attributeName)
    {
        if (Attributes != null &&
            Attributes.TryGetValue(attributeName, out var values) &&
            values != null &&
            values.Any())
        {
            return values[0];
        }
        return null;
    }

    // Helper method to set attribute value
    public UserProfileRepresentation WithAttribute(string name, string? value)
    {
        var newAttributes = Attributes != null
            ? new Dictionary<string, List<string>>(Attributes)
            : new Dictionary<string, List<string>>();

        if (value != null)
        {
            newAttributes[name] = new List<string> { value };
        }
        else
        {
            newAttributes.Remove(name);
        }

        return this with { Attributes = newAttributes };
    }

    // Factory method to create from your domain model
    public static UserProfileRepresentation FromUserProfileModel(UserProfileModel model)
    {
        var attributes = new Dictionary<string, List<string>>();

        // Add all attributes based on your profile configuration
        AddAttributeIfNotEmpty(attributes, "gender", model.Gender.ToString());
        AddAttributeIfNotEmpty(attributes, "dateOfBirth", model.DateOfBirth.ToString(CultureInfo.InvariantCulture));
        AddAttributeIfNotEmpty(attributes, "bio", model.Bio);
        AddAttributeIfNotEmpty(attributes, "profilePictureUrl", model.ProfilePictureUrl);
        AddAttributeIfNotEmpty(attributes, "city", model.City);
        AddAttributeIfNotEmpty(attributes, "country", model.Country);
        AddAttributeIfNotEmpty(attributes, "interests", model.Interests);

        if (model.Latitude.HasValue)
        {
            AddAttributeIfNotEmpty(attributes, "latitude", model.Latitude.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (model.Longitude.HasValue)
        {
            AddAttributeIfNotEmpty(attributes, "longitude", model.Longitude.Value.ToString(CultureInfo.InvariantCulture));
        }

        return new UserProfileRepresentation(
            //IdentityId: model.IdentityId,
            Username: model.UserName,
            Email: model.Email,
            EmailVerified: true,
            Enabled: true,
            Attributes: attributes
        );
    }

    private static void AddAttributeIfNotEmpty(
        Dictionary<string, List<string>> attributes,
        string key,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            attributes[key] = new List<string> { value };
        }
    }

    // Factory method to create from registration model
    public static UserProfileRepresentation FromRegistrationModel(UserModel model, string password)
    {
        var attributes = new Dictionary<string, List<string>>
        {
            ["gender"] = new List<string> { model.Gender.ToString() },
            ["dateOfBirth"] = new List<string> { model.DateOfBirth.ToString(CultureInfo.InvariantCulture) }
        };

        return new UserProfileRepresentation(
            //IdentityId: Guid.Empty, // Will be generated by Keycloak
            Username: model.Username,
            Email: model.Email,
            EmailVerified: false,
            Enabled: true,
            Attributes: attributes
            //Credentials: new List<CredentialRepresentation>
            //{
            //    new CredentialRepresentation("password", password, false)
            //}
        );
    }
}
