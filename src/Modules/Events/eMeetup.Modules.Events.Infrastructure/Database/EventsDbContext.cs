using System.Net.Sockets;
using eMeetup.Common.Infrastructure.Inbox;
using eMeetup.Common.Infrastructure.Outbox;
using eMeetup.Modules.Events.Application.Abstractions.Data;
using eMeetup.Modules.Events.Domain.EventInterests;
using eMeetup.Modules.Events.Domain.Events;
using eMeetup.Modules.Events.Domain.Tags;
using eMeetup.Modules.Events.Infrastructure.Events;
using eMeetup.Modules.Events.Infrastructure.Tags;
using Microsoft.EntityFrameworkCore;

namespace eMeetup.Modules.Events.Infrastructure.Database;

//Add-Migration InitialMigration -Context EventsDbContext -Project eMeetup.Modules.Events.Infrastructure -OutputDir Database\Migrations
// update-database -Context EventsDbContext
public sealed class EventsDbContext(DbContextOptions<EventsDbContext> options) : DbContext(options), IUnitOfWork
{
    internal DbSet<Event> Events { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<EventTag> EventTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Events);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new EventConfiguration());

        modelBuilder.ApplyConfiguration(new EventConfiguration());
        modelBuilder.ApplyConfiguration(new TagGroupConfiguration());
        modelBuilder.ApplyConfiguration(new TagConfiguration());
    }
}
