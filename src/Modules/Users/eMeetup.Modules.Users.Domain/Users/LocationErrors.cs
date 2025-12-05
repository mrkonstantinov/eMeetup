using eMeetup.Common.Domain;

public static class LocationErrors
{
    public static Error InvalidLatitude =>
        Error.Validation("Location.InvalidLatitude", "Latitude must be between -90 and 90 degrees");

    public static Error InvalidLongitude =>
        Error.Validation("Location.InvalidLongitude", "Longitude must be between -180 and 180 degrees");

    public static Error InvalidCity =>
        Error.Validation("Location.InvalidCity", "City is required");

    public static Error CityTooLong =>
        Error.Validation("Location.CityTooLong", "City name cannot exceed 100 characters");

    public static Error InvalidCountry =>
        Error.Validation("Location.InvalidCountry", "Country is required");

    public static Error CountryTooLong =>
        Error.Validation("Location.CountryTooLong", "Country name cannot exceed 100 characters");

    public static Error InvalidCoordinates =>
        Error.Validation("Location.InvalidCoordinates", "Invalid coordinates provided");

    public static Error GeocodingFailed =>
        Error.Failure("Location.GeocodingFailed", "Failed to geocode the provided coordinates");
}
