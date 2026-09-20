using eMeetup.Modules.Users.Domain.Activities;
using eMeetup.Modules.Users.Domain.Photos;
using eMeetup.Modules.Users.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Modules.Users.Infrastructure.Users;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // ================================================================
        // === TABLE ===
        // ================================================================
        builder.ToTable("users", "users");

        // ================================================================
        // === PRIMARY KEY ===
        // ================================================================
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()");

        // ================================================================
        // === PROPERTIES (скалярные, НЕ JSON) ===
        // ================================================================
        builder.Property(u => u.IdentityId)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("identity_id");

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("username");

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("email");

        builder.Property(u => u.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasSentinel(UserStatus.Inactive)
            .HasDefaultValue(UserStatus.Inactive)
            .HasColumnName("status");

        builder.Property(u => u.ProfileCompleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnName("profile_completed");

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(u => u.LastActiveAt)
            .HasColumnName("last_active_at");

        builder.Property(u => u.LastStatusChangeAt)
            .HasColumnName("last_status_change_at");

        builder.Property(u => u.StatusReason)
            .HasMaxLength(500)
            .HasColumnName("status_reason");

        // ================================================================
        // === IGNORE COMPUTED PROPERTIES ===
        // ================================================================
        builder.Ignore(u => u.Photos);
        builder.Ignore(u => u.ProfileImageUrl);

        // ================================================================
        // === USER PROFILE (JSON COLUMN) ===
        // ❌ НЕЛЬЗЯ использовать HasDefaultValue внутри JSON
        // ================================================================
        builder.OwnsOne(u => u.Profile, profile =>
        {
            profile.ToJson("profile");

            // --- Личная информация ---
            profile.Property(p => p.AvatarUrl).HasMaxLength(500);
            profile.Property(p => p.DateOfBirth);
            profile.Property(p => p.Bio).HasMaxLength(500);

            // --- Контакты ---
            profile.Property(p => p.Phone).HasMaxLength(20);
            profile.Property(p => p.Telegram).HasMaxLength(50);
            profile.Property(p => p.Instagram).HasMaxLength(50);

            // --- Местоположение ---
            profile.Property(p => p.City).HasMaxLength(100);
            profile.Property(p => p.Country).HasMaxLength(100);
            profile.Property(p => p.Latitude).HasPrecision(10, 8);
            profile.Property(p => p.Longitude).HasPrecision(11, 8);
            profile.Property(p => p.TimeZone).HasMaxLength(50);

            // --- Социальные характеристики ---
            profile.Property(p => p.Gender).HasMaxLength(20);
            profile.Property(p => p.Languages).HasMaxLength(100);
            profile.Property(p => p.Interests).HasMaxLength(500);

            // --- Статус ---
            // ❌ УБРАНО: .HasDefaultValue(true)
            profile.Property(p => p.IsPublic);

            // --- Верификация ---
            // ❌ УБРАНО: .HasDefaultValue(false)
            profile.Property(p => p.IsEmailVerified);
            profile.Property(p => p.IsPhoneVerified);

            // --- Метаданные ---
            profile.Property(p => p.LastProfileUpdate);

            // ============================================================
            // === ACTIVITY PREFERENCES (NESTED JSON) ===
            // ============================================================
            profile.OwnsOne(p => p.ActivityPreferences, prefs =>
            {
                prefs.Property(p => p.PreferredActivities)
                    .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(Enum.Parse<ActivityType>)
                            .ToHashSet())
                    .Metadata.SetValueComparer(new ValueComparer<IReadOnlySet<ActivityType>>(
                        (c1, c2) => c1!.SequenceEqual(c2!),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToHashSet()));

                prefs.Property(p => p.ActivityLevels)
                    .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(Enum.Parse<ActivityLevel>)
                            .ToHashSet())
                    .Metadata.SetValueComparer(new ValueComparer<IReadOnlySet<ActivityLevel>>(
                        (c1, c2) => c1!.SequenceEqual(c2!),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToHashSet()));

                prefs.Property(p => p.PreferredTimeOfDay)
                    .HasConversion<int?>();

                prefs.Property(p => p.PreferredDays)
                    .HasConversion(
                        v => string.Join(',', v.Select(d => (int)d)),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => (DayOfWeek)int.Parse(s))
                            .ToArray())
                    .Metadata.SetValueComparer(new ValueComparer<DayOfWeek[]>(
                        (c1, c2) => c1!.SequenceEqual(c2!),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToArray()));

                prefs.Property(p => p.MaxDistanceKm);
                prefs.Property(p => p.MinParticipants);
                prefs.Property(p => p.MaxParticipants);
            });

            // ============================================================
            // === AVAILABILITY STATUS (NESTED JSON) ===
            // ============================================================
            profile.OwnsOne(p => p.AvailabilityStatus, status =>
            {
                status.Property(s => s.Type).HasConversion<int>();
                status.Property(s => s.AvailableFrom);
                status.Property(s => s.AvailableUntil);
                status.Property(s => s.StatusMessage).HasMaxLength(200);
            });
        });

        // ================================================================
        // === PHOTOS COLLECTION (OWNED MANY, отдельная таблица) ===
        // ================================================================
        builder.OwnsMany<UserPhoto>("_photos", photo =>
        {
            photo.ToTable("user_photos", "users");
            photo.WithOwner().HasForeignKey("user_id");

            photo.Property(p => p.Id)
                .IsRequired()
                .HasDefaultValueSql("gen_random_uuid()");

            photo.Property(p => p.UserId)
                .IsRequired()
                .HasColumnName("user_id");

            photo.Property(p => p.Url)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("url");

            photo.Property(p => p.ThumbnailUrl).HasMaxLength(500).HasColumnName("thumbnail_url");
            photo.Property(p => p.FileName).HasMaxLength(200).HasColumnName("file_name");
            photo.Property(p => p.FileSize).HasColumnName("file_size");
            photo.Property(p => p.ContentType).HasMaxLength(100).HasColumnName("content_type");

            photo.Property(p => p.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnName("display_order");

            photo.Property(p => p.IsPrimary)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("is_primary");

            photo.Property(p => p.UploadedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("uploaded_at");

            photo.HasIndex("user_id").HasDatabaseName("ix_user_photos_user_id");
            photo.HasIndex("user_id", nameof(UserPhoto.IsPrimary))
                .HasDatabaseName("ix_user_photos_user_id_is_primary")
                .HasFilter("\"is_primary\" = true");
            photo.HasIndex(p => p.DisplayOrder).HasDatabaseName("ix_user_photos_display_order");
        });

        // ================================================================
        // === FAVORITES ===
        // ================================================================
        builder.HasMany(u => u.Favorites)
            .WithOne(f => f.User)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.FavoritedBy)
            .WithOne(f => f.TargetUser)
            .HasForeignKey(f => f.TargetUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ================================================================
        // === SUBSCRIPTIONS ===
        // ================================================================
        builder.HasMany(u => u.Subscriptions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Subscribers)
            .WithOne(s => s.TargetUser)
            .HasForeignKey(s => s.TargetUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ================================================================
        // === INDEXES ===
        // ================================================================
        builder.HasIndex(u => u.IdentityId).IsUnique().HasDatabaseName("ix_users_identity_id");
        builder.HasIndex(u => u.Username).IsUnique().HasDatabaseName("ix_users_username");
        builder.HasIndex(u => u.Email).IsUnique().HasDatabaseName("ix_users_email");
        builder.HasIndex(u => u.Status).HasDatabaseName("ix_users_status");
        builder.HasIndex(u => u.ProfileCompleted).HasDatabaseName("ix_users_profile_completed");
        builder.HasIndex(u => u.CreatedAt).HasDatabaseName("ix_users_created_at");
        builder.HasIndex(u => u.LastActiveAt).HasDatabaseName("ix_users_last_active_at");
        builder.HasIndex(u => new { u.Status, u.ProfileCompleted })
            .HasDatabaseName("ix_users_status_profile_completed");

        // ================================================================
        // === QUERY FILTER ===
        // ================================================================
        builder.HasQueryFilter(u => u.Status != UserStatus.Deleted);
    }
}
