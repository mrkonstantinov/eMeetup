using eMeetup.Modules.Users.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Modules.Users.Infrastructure.Users;

internal sealed class UserFavoriteConfiguration : IEntityTypeConfiguration<UserFavorite>
{
    public void Configure(EntityTypeBuilder<UserFavorite> builder)
    {
        builder.ToTable("user_favorites", "users");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id).IsRequired().HasDefaultValueSql("gen_random_uuid()");
        builder.Property(f => f.UserId).IsRequired();
        builder.Property(f => f.TargetUserId).IsRequired();
        builder.Property(f => f.AddedAt).IsRequired();
        builder.Property(f => f.RemovedAt);
        builder.Property(f => f.IsActive).IsRequired().HasDefaultValue(true);

        builder.HasOne(f => f.User)
            .WithMany(u => u.Favorites)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.TargetUser)
            .WithMany(u => u.FavoritedBy)
            .HasForeignKey(f => f.TargetUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => new { f.UserId, f.TargetUserId })
            .IsUnique()
            .HasFilter("\"is_active\" = true")
            .HasDatabaseName("ix_user_favorites_user_target_active");

        builder.HasIndex(f => f.IsActive).HasDatabaseName("ix_user_favorites_is_active");

        // === QUERY FILTER (согласован с User) ===
        builder.HasQueryFilter(f => f.User.Status != UserStatus.Deleted);
    }
}
