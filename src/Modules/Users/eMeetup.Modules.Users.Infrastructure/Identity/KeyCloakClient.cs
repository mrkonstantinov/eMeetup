using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using eMeetup.Modules.Users.Domain.Users;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace eMeetup.Modules.Users.Infrastructure.Identity;

internal sealed class KeyCloakClient(HttpClient httpClient)
{
    internal async Task<string> RegisterUserAsync(UserRepresentation user, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync(
            "users",
            user,
            cancellationToken);

        httpResponseMessage.EnsureSuccessStatusCode();

        return ExtractIdentityIdFromLocationHeader(httpResponseMessage);
    }

    // Method to update user profile
    public async Task UpdateUserAsync(Guid identityId, UserProfileRepresentation user, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = null;
        try
        {
            response = await httpClient.PutAsJsonAsync(
                $"users/{identityId}",
                user,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return;
            }

            // Read error response
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var statusCode = response.StatusCode;

            // Try to parse Keycloak error if it's JSON
            string errorMessage = "Unknown error";
            try
            {
                var errorObject = JsonSerializer.Deserialize<JsonElement>(errorContent);
                if (errorObject.TryGetProperty("errorMessage", out var errorMessageProp))
                {
                    errorMessage = errorMessageProp.GetString();
                }
            }
            catch
            {
                // If not JSON, use raw content
                errorMessage = errorContent;
            }

            // Throw custom exception with all details
            throw new KeycloakApiException(
                $"Failed to update user {identityId}. Status: {statusCode}. Error: {errorMessage}")
            {
                StatusCode = statusCode,
                ResponseContent = errorContent,
                RequestUri = response.RequestMessage?.RequestUri
            };
        }
        catch (HttpRequestException ex) when (ex.InnerException is TaskCanceledException)
        {
            throw new Exception("Request timed out", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Network error occurred: {ex.Message}", ex);
        }
        finally
        {
            response?.Dispose();
        }
    }

    private static string ExtractIdentityIdFromLocationHeader(HttpResponseMessage httpResponseMessage)
    {
        const string usersSegmentName = "users/";

        string? locationHeader = httpResponseMessage.Headers.Location?.PathAndQuery;

        if (locationHeader is null)
        {
            throw new InvalidOperationException("Location header is null");
        }

        int userSegmentValueIndex = locationHeader.IndexOf(
            usersSegmentName,
            StringComparison.InvariantCultureIgnoreCase);

        string identityId = locationHeader.Substring(userSegmentValueIndex + usersSegmentName.Length);

        return identityId;
    }

    public async Task<UserProfileRepresentation> GetUserAsync(Guid identityId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(
            $"users/{identityId}",
            cancellationToken);

        httpResponseMessage.EnsureSuccessStatusCode();

        return await httpResponseMessage.Content.ReadFromJsonAsync<UserProfileRepresentation>(
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize user response");
    }

    // Optional: Add method to get user by email or username
    public async Task<UserProfileRepresentation?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"users?email={Uri.EscapeDataString(email)}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<UserProfileRepresentation>>(cancellationToken: cancellationToken);
        return users?.FirstOrDefault();
    }
}


public class KeycloakApiException : Exception
{
    public HttpStatusCode? StatusCode { get; set; }
    public string ResponseContent { get; set; }
    public Uri RequestUri { get; set; }

    public KeycloakApiException(string message) : base(message) { }
}
