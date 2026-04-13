using eMeetup.Common.Infrastructure.Inbox;
using eMeetup.Common.Infrastructure.Outbox;
using eMeetup.Modules.Enrolls.Application.Abstractions.Data;
using eMeetup.Modules.Enrolls.Domain.Customers;
using eMeetup.Modules.Enrolls.Infrastructure.Customers;
using Microsoft.EntityFrameworkCore;

namespace eMeetup.Modules.Enrolls.Infrastructure.Database;

//Add-Migration InitialMigration -Context EnrollsDbContext -Project eMeetup.Modules.Enrolls.Infrastructure -OutputDir Database\Migrations
public sealed class EnrollsDbContext(DbContextOptions<EnrollsDbContext> options) : DbContext(options), IUnitOfWork
{
    internal DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Enrolls);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());

        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
    }
}
