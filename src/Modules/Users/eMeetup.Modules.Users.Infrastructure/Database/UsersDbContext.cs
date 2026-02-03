using eMeetup.Common.Infrastructure.Inbox;
using eMeetup.Common.Infrastructure.Outbox;
using eMeetup.Modules.Users.Application.Abstractions.Data;
using eMeetup.Modules.Users.Domain.Users;
using eMeetup.Modules.Users.Infrastructure.Photos;
using eMeetup.Modules.Users.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;

namespace eMeetup.Modules.Users.Infrastructure.Database;

//Add-Migration InitialMigration -Context UsersDbContext -Project eMeetup.Modules.Users.Infrastructure -OutputDir Database\Migrations
// update-database -Context UsersDbContext
public sealed class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<UserPhoto> UserPhotos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Users);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserPhotoConfiguration());
        modelBuilder.ApplyConfiguration(new UserInterestConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new TagConfiguration());      
    }
}
