using System.Globalization;
using System.Text.Json.Serialization;
using eMeetup.Modules.Users.Application.Abstractions.Identity;
using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Infrastructure.Identity;

internal sealed record UserRepresentation(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("enabled")] bool Enabled,
    [property: JsonPropertyName("emailVerified")] bool EmailVerified,
    [property: JsonPropertyName("credentials")] List<CredentialRepresentation>? Credentials);
