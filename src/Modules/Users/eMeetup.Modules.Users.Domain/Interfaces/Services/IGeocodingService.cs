using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Domain.Interfaces.Services;
public interface IGeocodingService
{
    Task<Result<string>> ReverseGeocodeAsync(double latitude, double longitude, CancellationToken cancellationToken = default);
    Task<Result<string>> GeocodeAsync(string address, CancellationToken cancellationToken = default);
}
