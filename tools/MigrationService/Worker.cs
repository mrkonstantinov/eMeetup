using MigrationService.Initializers;
using System.Diagnostics;

namespace MigrationService;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<Worker> logger) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource _activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            // TODO: Check that this is no longer needed in Aspire .NET 9
            // logger.LogInformation("Waiting for SQL Server to be ready");
            // await Task.Delay(10_000, cancellationToken);

            var sw = Stopwatch.StartNew();
            using var scope = serviceProvider.CreateScope();
            var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

            var usersInitializer = scope.ServiceProvider.GetRequiredService<UsersDbContextInitializer>();
            await usersInitializer.EnsureDatabaseAsync(cancellationToken);
            await usersInitializer.RunMigrationAsync(cancellationToken);

            if (environment.IsDevelopment())
            {
                await usersInitializer.SeedDataAsync(cancellationToken);
            }

            var eventsInitializer = scope.ServiceProvider.GetRequiredService<EventsDbContextInitializer>();
            await eventsInitializer.EnsureDatabaseAsync(cancellationToken);
            await eventsInitializer.RunMigrationAsync(cancellationToken);

            var enrollsInitializer = scope.ServiceProvider.GetRequiredService<EnrollsDbContextInitializer>();
            await enrollsInitializer.EnsureDatabaseAsync(cancellationToken);
            await enrollsInitializer.RunMigrationAsync(cancellationToken);

            var attendanceInitializer = scope.ServiceProvider.GetRequiredService<AttendanceDbContextInitializer>();
            await attendanceInitializer.EnsureDatabaseAsync(cancellationToken);
            await attendanceInitializer.RunMigrationAsync(cancellationToken);

            sw.Stop();
            logger.LogInformation($"DB creation and seeding took {sw.Elapsed} ");
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }
}
