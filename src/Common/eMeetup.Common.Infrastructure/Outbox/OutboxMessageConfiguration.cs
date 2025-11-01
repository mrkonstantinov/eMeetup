using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Common.Infrastructure.Outbox;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
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


        builder.Property(o => o.Content).HasMaxLength(2000).HasColumnType("jsonb");
    }
}
