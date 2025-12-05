using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Errors;
using eMeetup.Modules.Users.Domain.Interfaces.Services;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.Extensions.Logging;

namespace eMeetup.Modules.Users.Infrastructure.Services;

public class MockGeocodingService : IGeocodingService
{
    private readonly ILogger<MockGeocodingService> _logger;

    public MockGeocodingService(ILogger<MockGeocodingService> logger)
    {
        _logger = logger;
        _logger.LogInformation("Mock Geocoding Service initialized");
    }

    public async Task<Result<Location>> ReverseGeocodeAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken); // Simulate API call

        if (!ValidateCoordinates(latitude, longitude))
        {
            return Result.Failure<Location>(GeocodingErrors.InvalidCoordinates);
        }

        // Mock data based on coordinates
        var city = GetMockCity(latitude, longitude);
        var country = GetMockCountry(latitude, longitude);

        var location = Location.Create(latitude, longitude, city, country).Value;

        _logger.LogInformation("Mock reverse geocoding: {Latitude}, {Longitude} -> {City}, {Country}",
            latitude, longitude, city, country);

        return location;
    }

    public async Task<Result<Location>> GeocodeAsync(
        string address,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken); // Simulate API call

        if (string.IsNullOrWhiteSpace(address))
        {
            return Result.Failure<Location>(GeocodingErrors.EmptyAddress);
        }

        // Mock coordinates based on address
        var (latitude, longitude) = GetMockCoordinates(address);
        var location = Location.Create(latitude, longitude, "Mock City", "Mock Country").Value;

        _logger.LogInformation("Mock geocoding: {Address} -> {Latitude}, {Longitude}",
            address, latitude, longitude);

        return location;
    }

    public bool ValidateCoordinates(double latitude, double longitude)
    {
        return latitude >= -90 && latitude <= 90 &&
               longitude >= -180 && longitude <= 180;
    }

    private static string GetMockCity(double latitude, double longitude)
    {
        return (latitude, longitude) switch
        {
            ( > 40.0, < -70.0) => "New York",
            ( > 34.0, < -118.0) => "Los Angeles",
            ( > 51.0, < 0.0) => "London",
            ( > 48.0, < 2.0) => "Paris",
            ( > 35.0, < 139.0) => "Tokyo",
            _ => "Unknown City"
        };
    }

    private static string GetMockCountry(double latitude, double longitude)
    {
        return (latitude, longitude) switch
        {
            ( > 24.0 and < 50.0, > -125.0 and < -65.0) => "United States",
            ( > 50.0 and < 60.0, > -10.0 and < 2.0) => "United Kingdom",
            ( > 41.0 and < 52.0, > -5.0 and < 10.0) => "France",
            ( > 35.0 and < 46.0, > 125.0 and < 146.0) => "Japan",
            _ => "Unknown Country"
        };
    }

    private static (double latitude, double longitude) GetMockCoordinates(string address)
    {
        var hash = address.GetHashCode();
        var random = new Random(hash);

        var latitude = random.NextDouble() * 180 - 90;
        var longitude = random.NextDouble() * 360 - 180;

        return (latitude, longitude);
    }
}
