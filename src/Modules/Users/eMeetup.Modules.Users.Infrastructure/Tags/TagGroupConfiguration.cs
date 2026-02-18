using eMeetup.Modules.Users.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Modules.Users.Infrastructure.Tags;

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
        var waterSportsGroup = TagGroup.CreateForSeeding(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Water Sports",
            "Activities related to water-based recreation",
            "🌊",
            1
        ).Value;

        var hikingGroup = TagGroup.CreateForSeeding(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Hiking & Trekking",
            "Trail walking, backpacking, and mountain hiking",
            "🥾",
            2
        ).Value;

        var campingGroup = TagGroup.CreateForSeeding(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            "Camping",
            "Overnight outdoor stays and wilderness camping",
            "⛺",
            3
        ).Value;

        var climbingGroup = TagGroup.CreateForSeeding(
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            "Climbing",
            "Rock climbing, bouldering, and mountaineering",
            "🧗",
            4
        ).Value;

        var winterSportsGroup = TagGroup.CreateForSeeding(
            Guid.Parse("55555555-5555-5555-5555-555555555555"),
            "Winter Sports",
            "Snow and ice activities",
            "⛷️",
            5
        ).Value;

        var cyclingGroup = TagGroup.CreateForSeeding(
            Guid.Parse("66666666-6666-6666-6666-666666666666"),
            "Cycling",
            "Biking, mountain biking, and cycling tours",
            "🚴",
            6
        ).Value;

        var wildlifeGroup = TagGroup.CreateForSeeding(
            Guid.Parse("77777777-7777-7777-7777-777777777777"),
            "Wildlife & Nature",
            "Bird watching, nature photography, and wildlife observation",
            "🦌",
            7
        ).Value;

        var fishingGroup = TagGroup.CreateForSeeding(
            Guid.Parse("88888888-8888-8888-8888-888888888888"),
            "Fishing",
            "Angling, fly fishing, and ice fishing",
            "🎣",
            8
        ).Value;

        builder.HasData(
            waterSportsGroup,
            hikingGroup,
            campingGroup,
            climbingGroup,
            winterSportsGroup,
            cyclingGroup,
            wildlifeGroup,
            fishingGroup
        );
    }
}
