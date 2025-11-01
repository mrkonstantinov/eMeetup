using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Common.Infrastructure.Inbox;

public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages");

        builder.Property<Guid>("Id")
             .HasColumnName("id");

        builder.Property(o => o.Content)
            .HasColumnName("content");

        builder.Property(o => o.Error)
            .HasColumnName("error");

        builder.Property(o => o.OccurredOnUtc)
            .HasColumnName("occurred_on_utc");

        builder.Property(o => o.ProcessedOnUtc)
            .HasColumnName("processed_on_utc");

        builder.Property(o => o.Type)
            .HasColumnName("type");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Content).HasMaxLength(2000).HasColumnType("jsonb");
    }
}
