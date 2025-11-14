namespace eMeetup.Modules.Users.Infrastructure.Identity;

internal sealed record UserRepresentation(
    string Username,
    string Email,
    Dictionary<string, List<string>> Attributes,
    bool EmailVerified,
    bool Enabled,
    CredentialRepresentation[] Credentials);


internal sealed record UserProfileRepresentation(
    string Email,
    Dictionary<string, List<string>> Attributes);
