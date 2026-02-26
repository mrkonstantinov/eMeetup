using System.Net.Sockets;
using eMeetup.Common.Infrastructure.Inbox;
using eMeetup.Common.Infrastructure.Outbox;
using eMeetup.Modules.Events.Application.Abstractions.Data;
using eMeetup.Modules.Events.Infrastructure.Events;
using eMeetup.Modules.Events.Domain.Events;

using Microsoft.EntityFrameworkCore;

namespace eMeetup.Modules.Events.Infrastructure.Database;

public sealed class EventsDbContext(DbContextOptions<EventsDbContext> options) : DbContext(options), IUnitOfWork
{
    internal DbSet<Event> Events { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Events);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new EventConfiguration());
    }
}
