using eMeetup.Modules.Events.Domain.EventSessions;
using eMeetup.Modules.Events.Domain.MateTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Modules.Events.Infrastructure.MateTypes;

internal sealed class MateTypeConfiguration : IEntityTypeConfiguration<MateType>
{
    public void Configure(EntityTypeBuilder<MateType> builder)
    {
        builder.ToTable("mate_types");

        builder.HasKey(mt => mt.Id);

        builder.Property(mt => mt.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(mt => mt.SessionId)
            .HasColumnName("event_session_id")
            .IsRequired();

        builder.Property(mt => mt.Title)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(mt => mt.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(mt => mt.AllocatedSlots)
            .HasColumnName("allocated_slots")
            .IsRequired();

        builder.Property(mt => mt.MinAge)
            .HasColumnName("min_age")
            .IsRequired(false);

        builder.Property(mt => mt.MaxAge)
            .HasColumnName("max_age")
            .IsRequired(false);

        builder.Property(mt => mt.Gender)
            .HasColumnName("gender")
            .HasConversion<int>()
            .IsRequired(false);

        builder.Property(mt => mt.PreferredGender)
            .HasColumnName("preferred_gender")
            .HasConversion<int>()
            .IsRequired(false);

        builder.Property(mt => mt.PreferredAgeRange)
            .HasColumnName("preferred_age_range")
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(mt => mt.Priority)
            .HasColumnName("priority")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(mt => mt.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(mt => mt.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired(false);


        builder.HasOne<EventSession>().WithMany()
            .HasForeignKey(mt => mt.SessionId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_mate_type_event_session");

        // Indexes
        builder.HasIndex(mt => mt.SessionId)
            .HasDatabaseName("ix_mate_types_event_session_id");

        builder.HasIndex(mt => new { mt.SessionId, mt.Priority })
            .HasDatabaseName("ix_mate_types_session_priority");

        builder.HasIndex(mt => mt.Gender)
            .HasDatabaseName("ix_mate_types_gender");

        // Ignore computed properties
        builder.Ignore(mt => mt.FilledSlots);
        builder.Ignore(mt => mt.AvailableSlots);
        builder.Ignore(mt => mt.IsFull);



        builder.HasOne<EventSession>().WithMany().HasForeignKey(t => t.SessionId);
    }
}
