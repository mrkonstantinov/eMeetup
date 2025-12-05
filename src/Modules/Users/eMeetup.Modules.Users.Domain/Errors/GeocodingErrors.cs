using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Errors;

public static class GeocodingErrors
{
    public static Error InvalidCoordinates =>
        Error.Validation("Geocoding.InvalidCoordinates", "Invalid coordinates provided");

    public static Error EmptyAddress =>
        Error.Validation("Geocoding.EmptyAddress", "Address cannot be empty");

    public static Error GeocodingFailed =>
        Error.Failure("Geocoding.GeocodingFailed", "Failed to geocode the provided address");

    public static Error ReverseGeocodingFailed =>
        Error.Failure("Geocoding.ReverseGeocodingFailed", "Failed to reverse geocode the provided coordinates");

    public static Error ServiceUnavailable =>
        Error.Failure("Geocoding.ServiceUnavailable", "Geocoding service is unavailable");

    public static Error RateLimitExceeded =>
        Error.Failure("Geocoding.RateLimitExceeded", "Geocoding service rate limit exceeded");

    public static Error NoResultsFound =>
        Error.NotFound("Geocoding.NoResultsFound", "No results found for the provided location");
}
