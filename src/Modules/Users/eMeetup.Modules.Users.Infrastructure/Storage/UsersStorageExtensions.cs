using eMeetup.Modules.Users.Application.Abstractions;
using eMeetup.Modules.Users.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eMeetup.Modules.Users.Infrastructure;

public static class UsersStorageExtensions
{
    public static IServiceCollection AddUsersStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IFileUploadService, LocalFileUploadService>();

        return services;
    }
}
