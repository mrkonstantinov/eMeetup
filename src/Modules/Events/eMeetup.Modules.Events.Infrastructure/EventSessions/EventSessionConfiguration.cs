using eMeetup.Modules.Events.Domain.EventSessions;
using eMeetup.Modules.Events.Domain.MateTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Modules.Events.Infrastructure.EventSessions;

internal sealed class EventSessionConfiguration : IEntityTypeConfiguration<EventSession>
{
    public void Configure(EntityTypeBuilder<EventSession> builder)
    {
        builder.ToTable("event_sessions");

        builder.HasKey(u => u.Id);

        builder.Property(es => es.Latitude)
            .HasColumnName("latitude")
            .HasPrecision(10, 8)
            .IsRequired(false);

        builder.Property(es => es.Longitude)
            .HasColumnName("longitude")
            .HasPrecision(11, 8)
            .IsRequired(false);

        builder.Property(t => t.Title)
            .IsRequired();

        // Relationship configurations
        builder.HasOne(es => es.Event)
            .WithMany(e => e.Sessions)
            .HasForeignKey(es => es.EventId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_event_session_event");

        // Indexes for performance
        builder.HasIndex(es => es.EventId)
            .HasDatabaseName("ix_event_sessions_event_id");

        builder.HasIndex(es => es.StartsAtUtc)
            .HasDatabaseName("ix_event_sessions_starts_at_utc");

        builder.HasIndex(es => new { es.EventId, es.Status })
            .HasDatabaseName("ix_event_sessions_event_status");

        builder.HasIndex(es => new { es.Status, es.StartsAtUtc })
            .HasDatabaseName("ix_event_sessions_status_start_date");

        // Query filters
        builder.HasQueryFilter(es => es.Status != EventSessionStatus.Canceled);
    }
}
