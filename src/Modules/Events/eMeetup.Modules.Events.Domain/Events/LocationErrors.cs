using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Domain;

namespace eMeetup.Modules.Events.Domain.Events;
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

    public static Error InvalidStreet =>
        Error.Validation("Location.InvalidStreet", "Street is required");

    public static Error StreetTooLong =>
        Error.Validation("Location.StreetTooLong", "Street name cannot exceed 250 characters");

    public static Error InvalidCoordinates =>
        Error.Validation("Location.InvalidCoordinates", "Invalid coordinates provided");

    public static Error GeocodingFailed =>
        Error.Failure("Location.GeocodingFailed", "Failed to geocode the provided coordinates");
}
