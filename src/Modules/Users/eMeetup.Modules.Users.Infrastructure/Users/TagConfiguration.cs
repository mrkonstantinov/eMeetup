using eMeetup.Modules.Users.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Modules.Users.Infrastructure.Users;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            //.HasColumnName("id")
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()"); // PostgreSQL UUID generation

        builder.Property(t => t.Name)
            //.HasColumnName("name")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Slug)
            //.HasColumnName("slug")
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(t => t.Description)
            //.HasColumnName("description")
            .HasMaxLength(200)
            .HasDefaultValue(string.Empty);

        builder.Property(t => t.UsageCount)
            //.HasColumnName("usage_count")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(t => t.UpdatedAt)
            //.HasColumnName("updated_at")
            .IsRequired(false)
            .HasColumnType("timestamp with time zone");

        builder.Property(t => t.IsActive)
            //.HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(t => t.Name).IsUnique().HasDatabaseName("ix_tags_name");
        builder.HasIndex(t => t.Slug).IsUnique().HasDatabaseName("ix_tags_slug");
        builder.HasIndex(t => t.IsActive).HasDatabaseName("ix_tags_is_active");
        builder.HasIndex(t => t.UsageCount).HasDatabaseName("ix_tags_usage_count");
    }
}
