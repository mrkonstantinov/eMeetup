using eMeetup.Modules.Enrolls.Infrastructure.Database;

namespace MigrationService.Initializers;

internal class EnrollsDbContextInitializer : DbContextInitializerBase<EnrollsDbContext>
{
    public EnrollsDbContextInitializer(EnrollsDbContext dbContext) : base(dbContext)
    {
    }
}
