using eMeetup.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace MigrationService.Initializers;

internal class UsersDbContextInitializer : DbContextInitializerBase<UsersDbContext>
{
    public UsersDbContextInitializer(UsersDbContext dbContext) : base(dbContext)
    {
    }

    public async Task SeedDataAsync(CancellationToken cancellationToken)
    {
        var strategy = DbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Seed the database
            await using var transaction = await DbContext.Database.BeginTransactionAsync(cancellationToken);
            //var categories = await SeedCategories();
            //await SeedEventsAsync(categories);
            await DbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }
}
