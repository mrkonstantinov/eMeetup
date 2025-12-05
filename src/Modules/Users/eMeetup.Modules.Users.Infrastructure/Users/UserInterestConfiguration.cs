using eMeetup.Modules.Users.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Modules.Users.Infrastructure.Users;

internal sealed class UserInterestConfiguration : IEntityTypeConfiguration<UserInterest>
{
    public void Configure(EntityTypeBuilder<UserInterest> builder)
    {
        builder.ToTable("user_interests", t => t.HasComment("Junction table for user interests and tags"));

        // Primary Key
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()"); // PostgreSQL UUID generation

        // UserId foreign key
        builder.Property(i => i.UserId)
            .IsRequired();

        // TagId foreign key
        builder.Property(i => i.TagId)
            .IsRequired();

        // CreatedAt property
        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        // Relationships
        builder.HasOne(i => i.User)
            .WithMany(u => u.Interests)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ui => ui.Tag)
                .WithMany(t => t.UserInterests) // MUST point to Tag.UserInterests
                .HasForeignKey(ui => ui.TagId) // Explicitly uses the TagId property
                .OnDelete(DeleteBehavior.Restrict);


        // Indexes
        builder.HasIndex(i => i.UserId)
            .HasDatabaseName("ix_user_interests_user_id")
            .HasMethod("hash");

        builder.HasIndex(i => i.TagId)
            .HasDatabaseName("ix_user_interests_tag_id")
            .HasMethod("hash");

        builder.HasIndex(i => new { i.UserId, i.TagId })
            .IsUnique()
            .HasDatabaseName("ix_user_interests_user_tag_unique");

        builder.HasIndex(i => i.CreatedAt)
            .HasDatabaseName("ix_user_interests_created_at")
            .HasMethod("brin");
    }
}
