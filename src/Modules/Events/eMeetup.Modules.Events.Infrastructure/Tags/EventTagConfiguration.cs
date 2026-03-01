using eMeetup.Modules.Events.Domain.EventInterests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Modules.Events.Infrastructure.Tags;

internal sealed class EventTagConfiguration : IEntityTypeConfiguration<EventTag>
{
    public void Configure(EntityTypeBuilder<EventTag> builder)
    {
        builder.ToTable("event_tags", t => t.HasComment("Junction table for events and tags"));

        // Primary Key
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()"); // PostgreSQL UUID generation

        // EventId foreign key
        builder.Property(i => i.EventId) 
            .IsRequired();

        // TagId foreign key
        builder.Property(i => i.TagId)
            .IsRequired();

        // Relationships
        builder.HasOne(i => i.Event)
            .WithMany(u => u.Tags)
            .HasForeignKey(i => i.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ui => ui.Tag)
                .WithMany(t => t.EventTags) // MUST point to Tag.EventTags
                .HasForeignKey(ui => ui.TagId) // Explicitly uses the TagId property
                .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(i => i.EventId)
            .HasDatabaseName("ix_event_events_event_id")
            .HasMethod("hash");

        builder.HasIndex(i => i.TagId)
            .HasDatabaseName("ix_event_events_tag_id")
            .HasMethod("hash");

        builder.HasIndex(i => new { i.EventId, i.TagId })
            .IsUnique()
            .HasDatabaseName("ix_event_events_tag_unique");
    }
}
