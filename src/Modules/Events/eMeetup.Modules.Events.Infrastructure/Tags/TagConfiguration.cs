using eMeetup.Modules.Events.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Modules.Events.Infrastructure.Tags;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags");

        // Primary Key
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()"); // PostgreSQL UUID generation

        // Name
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(50);

        // Slug
        builder.Property(t => t.Slug)
            .IsRequired()
            .HasMaxLength(60);

        // Description
        builder.Property(t => t.Description)
            .HasMaxLength(200)
            .HasDefaultValue(string.Empty);

        // UsageCount
        builder.Property(t => t.UsageCount)
            .IsRequired()
            .HasDefaultValue(0);

        // IsActive
        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // TagGroupId (Foreign Key)
        builder.Property(t => t.TagGroupId)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(t => t.Name)
            .IsUnique()
            .HasDatabaseName("ix_tags_name");

        builder.HasIndex(t => t.Slug)
            .IsUnique()
            .HasDatabaseName("ix_tags_slug");

        builder.HasIndex(t => t.IsActive)
            .HasDatabaseName("ix_tags_is_active");

        builder.HasIndex(t => t.UsageCount)
            .HasDatabaseName("ix_tags_usage_count");

        builder.HasIndex(t => t.TagGroupId)
            .HasDatabaseName("ix_tags_tag_group_id");

        // Relationship with TagGroup
        builder.HasOne(t => t.TagGroup)
            .WithMany(tg => tg.Tags)
            .HasForeignKey(t => t.TagGroupId)
            .OnDelete(DeleteBehavior.SetNull);

        // Get group IDs
        var activeLifestyleGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var winterActivitiesGroupId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var adventureOutdoorsGroupId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var waterAdventuresGroupId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var festivalsEventsGroupId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var travelExplorationGroupId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        var cultureEntertainmentGroupId = Guid.Parse("77777777-7777-7777-7777-777777777777");
        var chillHangoutGroupId = Guid.Parse("88888888-8888-8888-8888-888888888888");
        var sportsEventsGroupId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        // Create tags using seeding factory method
        var tags = new List<Tag>
        {
            // Active Lifestyle Tags
            Tag.CreateForSeeding(Guid.Parse("11111111-1111-1111-1111-aaaaaaaaaaaa"), "Running", "Jogging and running activities", activeLifestyleGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("11111111-1111-1111-1111-bbbbbbbbbbbb"), "Swimming", "Pool swimming and training", activeLifestyleGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("11111111-1111-1111-1111-cccccccccccc"), "Cycling", "Road and city bike rides", activeLifestyleGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("11111111-1111-1111-1111-dddddddddddd"), "Gym Workout", "Fitness and strength training", activeLifestyleGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("11111111-1111-1111-1111-eeeeeeeeeeee"), "Yoga", "Yoga and stretching sessions", activeLifestyleGroupId).Value,
        
            // Winter Activities Tags
            Tag.CreateForSeeding(Guid.Parse("22222222-2222-2222-2222-aaaaaaaaaaaa"), "Alpine Skiing", "Downhill skiing on slopes", winterActivitiesGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("22222222-2222-2222-2222-bbbbbbbbbbbb"), "Cross-Country Skiing", "Nordic skiing on trails", winterActivitiesGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("22222222-2222-2222-2222-cccccccccccc"), "Snowboarding", "Snowboarding on slopes and parks", winterActivitiesGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("22222222-2222-2222-2222-dddddddddddd"), "Ice Skating", "Skating on ice rinks", winterActivitiesGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("22222222-2222-2222-2222-eeeeeeeeeeee"), "Snow Tubing", "Snow sliding with tubes", winterActivitiesGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("22222222-2222-2222-2222-ffffffffffff"), "Winter Hiking", "Hiking with snowshoes", winterActivitiesGroupId).Value,
        
            // Adventure & Outdoors Tags
            Tag.CreateForSeeding(Guid.Parse("33333333-3333-3333-3333-aaaaaaaaaaaa"), "Hike", "Day hiking on trails", adventureOutdoorsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("33333333-3333-3333-3333-bbbbbbbbbbbb"), "Bike Touring", "Multi-day cycling adventures", adventureOutdoorsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("33333333-3333-3333-3333-cccccccccccc"), "Picnic", "Outdoor meals in nature", adventureOutdoorsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("33333333-3333-3333-3333-dddddddddddd"), "Camping", "Overnight stays in nature", adventureOutdoorsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("33333333-3333-3333-3333-eeeeeeeeeeee"), "Trail Running", "Running on nature trails", adventureOutdoorsGroupId).Value,
        
            // Water Adventures Tags
            Tag.CreateForSeeding(Guid.Parse("44444444-4444-4444-4444-aaaaaaaaaaaa"), "Packrafting", "Lightweight portable rafting", waterAdventuresGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("44444444-4444-4444-4444-bbbbbbbbbbbb"), "SUP", "Stand-Up Paddleboarding", waterAdventuresGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("44444444-4444-4444-4444-cccccccccccc"), "Kayaking", "Kayaking on rivers and lakes", waterAdventuresGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("44444444-4444-4444-4444-dddddddddddd"), "Canoeing", "Canoe trips on calm waters", waterAdventuresGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("44444444-4444-4444-4444-eeeeeeeeeeee"), "Rafting", "White water rafting", waterAdventuresGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("44444444-4444-4444-4444-ffffffffffff"), "Open Water Swimming", "Swimming in lakes and seas", waterAdventuresGroupId).Value,
        
            // Festivals & Events Tags
            Tag.CreateForSeeding(Guid.Parse("55555555-5555-5555-5555-aaaaaaaaaaaa"), "Music Festival", "Live music and concerts festivals", festivalsEventsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("55555555-5555-5555-5555-bbbbbbbbbbbb"), "Food Festival", "Culinary and gastronomy events", festivalsEventsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("55555555-5555-5555-5555-cccccccccccc"), "Cultural Festival", "Traditional and cultural celebrations", festivalsEventsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("55555555-5555-5555-5555-dddddddddddd"), "City Celebration", "Local city festivals and fairs", festivalsEventsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("55555555-5555-5555-5555-eeeeeeeeeeee"), "Beer Festival", "Craft beer and brewery events", festivalsEventsGroupId).Value,
        
            // Travel & Exploration Tags
            Tag.CreateForSeeding(Guid.Parse("66666666-6666-6666-6666-aaaaaaaaaaaa"), "City Trip", "Sightseeing in cities", travelExplorationGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("66666666-6666-6666-6666-bbbbbbbbbbbb"), "Food Tourism", "Gastronomy and local food tours", travelExplorationGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("66666666-6666-6666-6666-cccccccccccc"), "Road Trip", "Traveling by car", travelExplorationGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("66666666-6666-6666-6666-dddddddddddd"), "Cultural Tour", "Museums and architecture tours", travelExplorationGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("66666666-6666-6666-6666-eeeeeeeeeeee"), "Weekend Getaway", "Short trips out of town", travelExplorationGroupId).Value,
        
            // Culture & Entertainment Tags
            Tag.CreateForSeeding(Guid.Parse("77777777-7777-7777-7777-aaaaaaaaaaaa"), "Cinema", "Movies and film screenings", cultureEntertainmentGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("77777777-7777-7777-7777-bbbbbbbbbbbb"), "Theater", "Plays and theatrical performances", cultureEntertainmentGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("77777777-7777-7777-7777-cccccccccccc"), "Concert", "Live music performances", cultureEntertainmentGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("77777777-7777-7777-7777-dddddddddddd"), "Rock Show", "Rock and metal gigs", cultureEntertainmentGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("77777777-7777-7777-7777-eeeeeeeeeeee"), "Art Exhibition", "Gallery and art shows", cultureEntertainmentGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("77777777-7777-7777-7777-ffffffffffff"), "Comedy Show", "Stand-up comedy performances", cultureEntertainmentGroupId).Value,
        
            // Chill & Hangout Tags
            Tag.CreateForSeeding(Guid.Parse("88888888-8888-8888-8888-aaaaaaaaaaaa"), "Pub", "Bar and pub meetups", chillHangoutGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("88888888-8888-8888-8888-bbbbbbbbbbbb"), "Patio", "Outdoor cafes and terraces", chillHangoutGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("88888888-8888-8888-8888-cccccccccccc"), "Coffee Date", "Casual coffee meetings", chillHangoutGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("88888888-8888-8888-8888-dddddddddddd"), "Board Games", "Board game nights", chillHangoutGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("88888888-8888-8888-8888-eeeeeeeeeeee"), "Picnic in Park", "Relaxed park gatherings", chillHangoutGroupId).Value,
        
            // Sports Events Tags
            Tag.CreateForSeeding(Guid.Parse("99999999-9999-9999-9999-aaaaaaaaaaaa"), "Triathlon", "Swim, bike, run races", sportsEventsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("99999999-9999-9999-9999-bbbbbbbbbbbb"), "Marathon", "Running marathons in other cities", sportsEventsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("99999999-9999-9999-9999-cccccccccccc"), "Obstacle Race", "Tough mudder and obstacle courses", sportsEventsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("99999999-9999-9999-9999-dddddddddddd"), "Cycling Race", "Competitive bike racing", sportsEventsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("99999999-9999-9999-9999-eeeeeeeeeeee"), "Swimming Competition", "Open water or pool races", sportsEventsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("99999999-9999-9999-9999-ffffffffffff"), "Trail Running Race", "Off-road running competitions", sportsEventsGroupId).Value,
        };

        builder.HasData(tags);
    }
}
