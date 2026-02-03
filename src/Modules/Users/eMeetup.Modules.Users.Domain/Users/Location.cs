using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Domain.Users
{
    public sealed class Location : ValueObject
    {
        public double? Latitude { get; }
        public double? Longitude { get; }
        public string? City { get; }
        public string? Country { get; }

        public Location()
        {
            
        }

        private Location(double? latitude, double? longitude, string? city, string? country)
        {
            Latitude = latitude;
            Longitude = longitude;
            City = city;
            Country = country;
        }

        // Factory method for full location with validation
        public static Result<Location> Create(double? latitude, double? longitude, string? city, string? country)
        {
            // Validate coordinates if provided
            if (latitude.HasValue && (latitude.Value < -90 || latitude.Value > 90))
                return Result.Failure<Location>(LocationErrors.InvalidLatitude);

            if (longitude.HasValue && (longitude.Value < -180 || longitude.Value > 180))
                return Result.Failure<Location>(LocationErrors.InvalidLongitude);

            // Validate city if provided
            if (city != null && city.Length > 100)
                return Result.Failure<Location>(LocationErrors.CityTooLong);

            // Validate country if provided
            if (country != null && country.Length > 100)
                return Result.Failure<Location>(LocationErrors.CountryTooLong);

            // Trim strings if they exist
            var trimmedCity = string.IsNullOrWhiteSpace(city) ? null : city.Trim();
            var trimmedCountry = string.IsNullOrWhiteSpace(country) ? null : country.Trim();

            var location = new Location(
                latitude.HasValue ? Math.Round(latitude.Value, 6) : (double?)null,
                longitude.HasValue ? Math.Round(longitude.Value, 6) : (double?)null,
                trimmedCity,
                trimmedCountry);

            return Result.Success(location);
        }

        // Factory method with only coordinates
        public static Result<Location> Create(double latitude, double longitude)
        {
            return Create(latitude, longitude, null, null);
        }

        // Factory method with only city and country
        public static Result<Location> Create(string city, string country)
        {
            return Create(null, null, city, country);
        }

        // Factory method for coordinates only (nullable)
        public static Result<Location> Create(double? latitude, double? longitude)
        {
            return Create(latitude, longitude, null, null);
        }

        // Factory method for creating from existing location with updates
        public static Result<Location> FromExisting(Location existing,
            double? latitude = null,
            double? longitude = null,
            string? city = null,
            string? country = null)
        {
            return Create(
                latitude ?? existing.Latitude,
                longitude ?? existing.Longitude,
                city ?? existing.City,
                country ?? existing.Country
            );
        }

        // Methods for updating location
        public Result<Location> WithCoordinates(double? latitude, double? longitude)
        {
            return Create(latitude, longitude, City, Country);
        }

        public Result<Location> WithCity(string? city)
        {
            return Create(Latitude, Longitude, city, Country);
        }

        public Result<Location> WithCountry(string? country)
        {
            return Create(Latitude, Longitude, City, country);
        }

        public Result<Location> WithCityAndCountry(string? city, string? country)
        {
            return Create(Latitude, Longitude, city, country);
        }

        // Business logic methods
        public bool HasCoordinates() => Latitude.HasValue && Longitude.HasValue;

        public bool HasCity() => !string.IsNullOrWhiteSpace(City);

        public bool HasCountry() => !string.IsNullOrWhiteSpace(Country);

        public bool IsComplete() => HasCoordinates() && HasCity() && HasCountry();

        public bool IsValid()
        {
            var validLatitude = !Latitude.HasValue || (Latitude.Value >= -90 && Latitude.Value <= 90);
            var validLongitude = !Longitude.HasValue || (Longitude.Value >= -180 && Longitude.Value <= 180);
            var validCity = City == null || City.Length <= 100;
            var validCountry = Country == null || Country.Length <= 100;

            return validLatitude && validLongitude && validCity && validCountry;
        }

        public double? CalculateDistanceTo(Location other)
        {
            if (!HasCoordinates() || !other.HasCoordinates())
                return null;

            // Haversine formula to calculate distance between two points
            const double earthRadiusKm = 6371;

            var lat1 = Latitude!.Value;
            var lon1 = Longitude!.Value;
            var lat2 = other.Latitude!.Value;
            var lon2 = other.Longitude!.Value;

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return earthRadiusKm * c;
        }

        public bool? IsWithinRadius(Location other, double radiusKm)
        {
            var distance = CalculateDistanceTo(other);
            return distance.HasValue ? distance.Value <= radiusKm : (bool?)null;
        }

        public string ToCoordinateString()
        {
            return HasCoordinates()
                ? $"{Latitude!.Value},{Longitude!.Value}"
                : "Coordinates not available";
        }

        public string ToLocationString()
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(City))
                parts.Add(City!);

            if (!string.IsNullOrWhiteSpace(Country))
                parts.Add(Country!);

            return parts.Count > 0
                ? string.Join(", ", parts)
                : "Location not specified";
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180;

        // Value object equality
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Latitude;
            yield return Longitude;
            yield return City;
            yield return Country;
        }

        // Helper methods for dictionary/list creation
        public List<string> GetLatitudeAsStringList()
        {
            return Latitude.HasValue
                ? new List<string> { Latitude.Value.ToString(CultureInfo.InvariantCulture) }
                : new List<string>();
        }

        public List<string> GetLongitudeAsStringList()
        {
            return Longitude.HasValue
                ? new List<string> { Longitude.Value.ToString(CultureInfo.InvariantCulture) }
                : new List<string>();
        }

        public List<string> GetCityAsStringList()
        {
            return !string.IsNullOrWhiteSpace(City)
                ? new List<string> { City! }
                : new List<string>();
        }

        public List<string> GetCountryAsStringList()
        {
            return !string.IsNullOrWhiteSpace(Country)
                ? new List<string> { Country! }
                : new List<string>();
        }

        public override string ToString()
        {
            var coordinatePart = HasCoordinates() ? $" ({Latitude}, {Longitude})" : "";
            var locationPart = ToLocationString();

            return $"{locationPart}{coordinatePart}";
        }
    }
}
