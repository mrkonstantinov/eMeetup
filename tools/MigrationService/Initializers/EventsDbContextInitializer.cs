using eMeetup.Modules.Events.Infrastructure.Database;

namespace MigrationService.Initializers;

internal class EventsDbContextInitializer : DbContextInitializerBase<EventsDbContext>
{
    public EventsDbContextInitializer(EventsDbContext dbContext) : base(dbContext)
    {
    }
}
