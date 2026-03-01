using eMeetup.Modules.Events.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Modules.Events.Infrastructure.Tags;

public class TagGroupConfiguration : IEntityTypeConfiguration<TagGroup>
{
    public void Configure(EntityTypeBuilder<TagGroup> builder)
    {
        builder.ToTable("tag_groups");

        // Primary Key
        builder.HasKey(tg => tg.Id);
        builder.Property(tg => tg.Id)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()"); // PostgreSQL UUID generation

        // Name
        builder.Property(tg => tg.Name)
            .IsRequired()
            .HasMaxLength(50);

        // Description
        builder.Property(tg => tg.Description)
            .HasMaxLength(200)
            .HasDefaultValue(string.Empty);

        // Icon
        builder.Property(tg => tg.Icon)
            .HasMaxLength(50)
            .HasDefaultValue(string.Empty);

        // Display Order
        builder.Property(tg => tg.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // IsActive
        builder.Property(tg => tg.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(tg => tg.Name)
            .IsUnique()
            .HasDatabaseName("ix_tag_groups_name");

        builder.HasIndex(tg => tg.IsActive)
            .HasDatabaseName("ix_tag_groups_is_active");

        builder.HasIndex(tg => tg.DisplayOrder)
            .HasDatabaseName("ix_tag_groups_display_order");

        // Relationship with Tags (one-to-many)
        builder.HasMany(tg => tg.Tags)
            .WithOne(t => t.TagGroup)
            .HasForeignKey(t => t.TagGroupId)
            .OnDelete(DeleteBehavior.SetNull); // When a group is deleted, set TagGroupId to null for associated tags


        // Seed TagGroups using factory method
        var activeLifestyleGroup = TagGroup.CreateForSeeding(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Active Lifestyle",
            "Running, swimming, cycling and other sports activities",
            "🏃",
            1
        ).Value;

        var winterActivitiesGroup = TagGroup.CreateForSeeding(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Winter Activities",
            "Skiing, snowboarding, ice skating and snow tubing",
            "❄️",
            2
        ).Value;

        var adventureOutdoorsGroup = TagGroup.CreateForSeeding(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            "Adventure & Outdoors",
            "Hiking, biking tours and picnics in nature",
            "⛰️",
            3
        ).Value;

        var waterAdventuresGroup = TagGroup.CreateForSeeding(
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            "Water Adventures",
            "Packrafting, SUP, kayaking and other water hikes",
            "🚣",
            4
        ).Value;

        var festivalsEventsGroup = TagGroup.CreateForSeeding(
            Guid.Parse("55555555-5555-5555-5555-555555555555"),
            "Festivals & Events",
            "Music, food and cultural festivals celebrations",
            "🎪",
            5
        ).Value;

        var travelExplorationGroup = TagGroup.CreateForSeeding(
            Guid.Parse("66666666-6666-6666-6666-666666666666"),
            "Travel & Exploration",
            "City trips, sightseeing, food tours and road trips",
            "✈️",
            6
        ).Value;

        var cultureEntertainmentGroup = TagGroup.CreateForSeeding(
            Guid.Parse("77777777-7777-7777-7777-777777777777"),
            "Culture & Entertainment",
            "Cinema, theater, concerts and rock shows",
            "🎭",
            7
        ).Value;

        var chillHangoutGroup = TagGroup.CreateForSeeding(
            Guid.Parse("88888888-8888-8888-8888-888888888888"),
            "Chill & Hangout",
            "Pub meetups, terrace cafes and relaxing time",
            "😎",
            8
        ).Value;

        var sportsEventsGroup = TagGroup.CreateForSeeding(
            Guid.Parse("99999999-9999-9999-9999-999999999999"),
            "Sports Events",
            "Marathons, triathlons, competitions and races in other cities",
             "🏆",
            9
        ).Value;

        builder.HasData(
            activeLifestyleGroup,
            winterActivitiesGroup,
            adventureOutdoorsGroup,
            waterAdventuresGroup,
            festivalsEventsGroup,
            travelExplorationGroup,
            cultureEntertainmentGroup,
            chillHangoutGroup,
            sportsEventsGroup
        );
    }
}
