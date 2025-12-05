using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Domain.Users
{
    public sealed class Location : ValueObject
    {
        public double Latitude { get; }
        public double Longitude { get; }
        public string City { get; }
        public string Country { get; }

        private Location(double latitude, double longitude, string city, string country)
        {
            Latitude = latitude;
            Longitude = longitude;
            City = city;
            Country = country;
        }

        // Factory method with validation
        public static Result<Location> Create(double latitude, double longitude, string city, string country)
        {
            // Validate coordinates
            if (latitude < -90 || latitude > 90)
                return Result.Failure<Location>(LocationErrors.InvalidLatitude);

            if (longitude < -180 || longitude > 180)
                return Result.Failure<Location>(LocationErrors.InvalidLongitude);

            // Validate city
            if (string.IsNullOrWhiteSpace(city))
                return Result.Failure<Location>(LocationErrors.InvalidCity);

            if (city.Length > 100)
                return Result.Failure<Location>(LocationErrors.CityTooLong);

            // Validate country
            if (string.IsNullOrWhiteSpace(country))
                return Result.Failure<Location>(LocationErrors.InvalidCountry);

            if (country.Length > 100)
                return Result.Failure<Location>(LocationErrors.CountryTooLong);

            var location = new Location(
                Math.Round(latitude, 6), // Precision to 6 decimal places
                Math.Round(longitude, 6),
                city.Trim(),
                country.Trim());

            return Result.Success(location);
        }

        // Factory method with only coordinates
        public static Result<Location> Create(double latitude, double longitude)
        {
            return Create(latitude, longitude, "Unknown", "Unknown");
        }

        // Methods for updating location
        public Result<Location> WithCoordinates(double latitude, double longitude)
        {
            return Create(latitude, longitude, City, Country);
        }

        public Result<Location> WithCity(string city)
        {
            return Create(Latitude, Longitude, city, Country);
        }

        public Result<Location> WithCountry(string country)
        {
            return Create(Latitude, Longitude, City, country);
        }

        public Result<Location> WithCityAndCountry(string city, string country)
        {
            return Create(Latitude, Longitude, city, country);
        }

        // Business logic methods
        public bool IsValid() => Latitude >= -90 && Latitude <= 90 && Longitude >= -180 && Longitude <= 180;

        public double CalculateDistanceTo(Location other)
        {
            // Haversine formula to calculate distance between two points
            const double earthRadiusKm = 6371;

            var dLat = ToRadians(other.Latitude - Latitude);
            var dLon = ToRadians(other.Longitude - Longitude);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return earthRadiusKm * c;
        }

        public bool IsWithinRadius(Location other, double radiusKm)
        {
            return CalculateDistanceTo(other) <= radiusKm;
        }

        public string ToCoordinateString() => $"{Latitude},{Longitude}";

        private static double ToRadians(double degrees) => degrees * Math.PI / 180;

        // Value object equality - THIS WAS MISSING
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Latitude;
            yield return Longitude;
            yield return City;
            yield return Country;
        }

        public override string ToString()
        {
            return $"{City}, {Country} ({Latitude}, {Longitude})";
        }
    }
}
