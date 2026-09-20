using eMeetup.Modules.Users.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Modules.Users.Infrastructure.Users;

internal sealed class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> builder)
    {
        builder.ToTable("user_subscriptions", "users");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).IsRequired().HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.UserId).IsRequired();
        builder.Property(s => s.TargetUserId).IsRequired();
        builder.Property(s => s.Type).IsRequired().HasConversion<int>();
        builder.Property(s => s.SubscribedAt).IsRequired();
        builder.Property(s => s.UnsubscribedAt);
        builder.Property(s => s.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(s => s.LastNotificationSentAt);

        builder.HasOne(s => s.User)
            .WithMany(u => u.Subscriptions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.TargetUser)
            .WithMany(u => u.Subscribers)
            .HasForeignKey(s => s.TargetUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.UserId, s.TargetUserId, s.Type })
            .IsUnique()
            .HasFilter("\"is_active\" = true")
            .HasDatabaseName("ix_user_subscriptions_user_target_type_active");

        builder.HasIndex(s => s.IsActive).HasDatabaseName("ix_user_subscriptions_is_active");

        // === QUERY FILTER ===
        builder.HasQueryFilter(s => s.User.Status != UserStatus.Deleted);
    }
}
