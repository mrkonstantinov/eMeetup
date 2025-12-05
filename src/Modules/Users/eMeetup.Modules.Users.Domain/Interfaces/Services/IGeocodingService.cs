using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Errors;
using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Domain.Interfaces.Services;
public interface IGeocodingService
{
    Task<Result<Location>> ReverseGeocodeAsync(double latitude, double longitude, CancellationToken cancellationToken = default);
    Task<Result<Location>> GeocodeAsync(string address, CancellationToken cancellationToken = default);
}
