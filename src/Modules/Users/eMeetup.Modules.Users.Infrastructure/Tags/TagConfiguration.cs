using eMeetup.Modules.Users.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Modules.Users.Infrastructure.Tags;

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
        var waterSportsGroupId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var hikingGroupId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var campingGroupId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var climbingGroupId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var winterSportsGroupId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var cyclingGroupId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        var wildlifeGroupId = Guid.Parse("77777777-7777-7777-7777-777777777777");
        var fishingGroupId = Guid.Parse("88888888-8888-8888-8888-888888888888");

        // Create tags using seeding factory method
        var tags = new List<Tag>
        {
            // Water Sports Tags (using only a-f)
            Tag.CreateForSeeding(Guid.Parse("aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa"), "Kayaking", "Paddle through waters in a kayak", waterSportsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("aaaaaaaa-1111-1111-1111-bbbbbbbbbbbb"), "Rafting", "White water rafting adventures", waterSportsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("aaaaaaaa-1111-1111-1111-cccccccccccc"), "Surfing", "Ride the waves", waterSportsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("aaaaaaaa-1111-1111-1111-dddddddddddd"), "Scuba Diving", "Explore underwater worlds", waterSportsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("aaaaaaaa-1111-1111-1111-eeeeeeeeeeee"), "Snorkeling", "Surface underwater exploration", waterSportsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("aaaaaaaa-1111-1111-1111-ffffffffffff"), "Stand Up Paddleboarding", "Paddleboarding on calm waters", waterSportsGroupId).Value,
            // Changed: gggg -> 1111 (using only hex)
            Tag.CreateForSeeding(Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111"), "Waterskiing", "Skiing on water surface", waterSportsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("aaaaaaaa-1111-1111-1111-222222222222"), "Wakeboarding", "Board riding on wake", waterSportsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("aaaaaaaa-1111-1111-1111-333333333333"), "Sailing", "Wind-powered boating", waterSportsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("aaaaaaaa-1111-1111-1111-444444444444"), "Canoeing", "Paddle in open canoes", waterSportsGroupId).Value,

            // Hiking Tags (using only a-f in the second group)
            Tag.CreateForSeeding(Guid.Parse("bbbbbbbb-2222-2222-2222-aaaaaaaaaaaa"), "Day Hiking", "Single day trail walks", hikingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("bbbbbbbb-2222-2222-2222-bbbbbbbbbbbb"), "Backpacking", "Multi-day hiking with gear", hikingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("bbbbbbbb-2222-2222-2222-cccccccccccc"), "Trail Running", "Running on nature trails", hikingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("bbbbbbbb-2222-2222-2222-dddddddddddd"), "Peak Bagging", "Summiting mountain peaks", hikingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("bbbbbbbb-2222-2222-2222-eeeeeeeeeeee"), "Nature Walks", "Leisurely nature exploration", hikingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("bbbbbbbb-2222-2222-2222-ffffffffffff"), "Nordic Walking", "Walking with poles", hikingGroupId).Value,
            // Changed: gggg -> 1111
            Tag.CreateForSeeding(Guid.Parse("bbbbbbbb-2222-2222-2222-111111111111"), "Scrambling", "Technical hiking with hands-on climbing", hikingGroupId).Value,

            // Camping Tags
            Tag.CreateForSeeding(Guid.Parse("cccccccc-3333-3333-3333-aaaaaaaaaaaa"), "Tent Camping", "Traditional tent camping", campingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("cccccccc-3333-3333-3333-bbbbbbbbbbbb"), "RV Camping", "Camping with recreational vehicles", campingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("cccccccc-3333-3333-3333-cccccccccccc"), "Backcountry Camping", "Remote wilderness camping", campingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("cccccccc-3333-3333-3333-dddddddddddd"), "Glamping", "Luxury camping experience", campingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("cccccccc-3333-3333-3333-eeeeeeeeeeee"), "Hammock Camping", "Sleeping in hammocks", campingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("cccccccc-3333-3333-3333-ffffffffffff"), "Winter Camping", "Cold weather camping", campingGroupId).Value,
            // Changed: gggg -> 1111
            Tag.CreateForSeeding(Guid.Parse("cccccccc-3333-3333-3333-111111111111"), "Beach Camping", "Camping on or near beaches", campingGroupId).Value,

            // Climbing Tags
            Tag.CreateForSeeding(Guid.Parse("dddddddd-4444-4444-4444-aaaaaaaaaaaa"), "Rock Climbing", "Climbing natural rock formations", climbingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("dddddddd-4444-4444-4444-bbbbbbbbbbbb"), "Bouldering", "Low-level climbing without ropes", climbingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("dddddddd-4444-4444-4444-cccccccccccc"), "Sport Climbing", "Climbing with fixed anchors", climbingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("dddddddd-4444-4444-4444-dddddddddddd"), "Traditional Climbing", "Placing own protection while climbing", climbingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("dddddddd-4444-4444-4444-eeeeeeeeeeee"), "Ice Climbing", "Climbing frozen waterfalls and ice formations", climbingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("dddddddd-4444-4444-4444-ffffffffffff"), "Mountaineering", "Mountain climbing expeditions", climbingGroupId).Value,
            // Changed: gggg -> 1111
            Tag.CreateForSeeding(Guid.Parse("dddddddd-4444-4444-4444-111111111111"), "Via Ferrata", "Protected climbing routes with cables", climbingGroupId).Value,

            // Winter Sports Tags
            Tag.CreateForSeeding(Guid.Parse("eeeeeeee-5555-5555-5555-aaaaaaaaaaaa"), "Downhill Skiing", "Alpine skiing on slopes", winterSportsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("eeeeeeee-5555-5555-5555-bbbbbbbbbbbb"), "Cross-Country Skiing", "Nordic skiing on trails", winterSportsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("eeeeeeee-5555-5555-5555-cccccccccccc"), "Snowboarding", "Riding snow on a board", winterSportsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("eeeeeeee-5555-5555-5555-dddddddddddd"), "Snowshoeing", "Walking on snow with specialized footwear", winterSportsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("eeeeeeee-5555-5555-5555-eeeeeeeeeeee"), "Ice Skating", "Skating on frozen surfaces", winterSportsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("eeeeeeee-5555-5555-5555-ffffffffffff"), "Snowmobiling", "Riding motorized snow vehicles", winterSportsGroupId).Value,
            // Changed: gggg -> 1111
            Tag.CreateForSeeding(Guid.Parse("eeeeeeee-5555-5555-5555-111111111111"), "Backcountry Skiing", "Skiing in unpatrolled areas", winterSportsGroupId).Value,

            // Cycling Tags
            Tag.CreateForSeeding(Guid.Parse("ffffffff-6666-6666-6666-aaaaaaaaaaaa"), "Mountain Biking", "Off-road cycling on trails", cyclingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("ffffffff-6666-6666-6666-bbbbbbbbbbbb"), "Road Cycling", "Cycling on paved roads", cyclingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("ffffffff-6666-6666-6666-cccccccccccc"), "Gravel Cycling", "Cycling on gravel roads", cyclingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("ffffffff-6666-6666-6666-dddddddddddd"), "BMX", "Bicycle motocross", cyclingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("ffffffff-6666-6666-6666-eeeeeeeeeeee"), "Cyclocross", "Mixed terrain cycling with obstacles", cyclingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("ffffffff-6666-6666-6666-ffffffffffff"), "Fat Biking", "Cycling with oversized tires on soft surfaces", cyclingGroupId).Value,
            // Changed: gggg -> 1111
            Tag.CreateForSeeding(Guid.Parse("ffffffff-6666-6666-6666-111111111111"), "Bike Touring", "Multi-day cycling trips with luggage", cyclingGroupId).Value,

            // Wildlife & Nature Tags - USING DIFFERENT PREFIX FOR THIS GROUP
            // Changed: gggggggg -> aaaaaaaa (since 'g' is invalid)
            Tag.CreateForSeeding(Guid.Parse("aaaaaaaa-7777-7777-7777-aaaaaaaaaaaa"), "Bird Watching", "Observing birds in their habitat", wildlifeGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("aaaaaaaa-7777-7777-7777-bbbbbbbbbbbb"), "Wildlife Photography", "Photographing animals in nature", wildlifeGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("aaaaaaaa-7777-7777-7777-cccccccccccc"), "Nature Photography", "Capturing landscapes and natural scenes", wildlifeGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("aaaaaaaa-7777-7777-7777-dddddddddddd"), "Stargazing", "Observing celestial objects", wildlifeGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("aaaaaaaa-7777-7777-7777-eeeeeeeeeeee"), "Botany", "Studying plants and wildflowers", wildlifeGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("aaaaaaaa-7777-7777-7777-ffffffffffff"), "Geocaching", "Outdoor treasure hunting with GPS", wildlifeGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("aaaaaaaa-7777-7777-7777-111111111111"), "Foraging", "Searching for wild food", wildlifeGroupId).Value,

            // Fishing Tags - USING DIFFERENT PREFIX FOR THIS GROUP
            // Changed: hhhhhhhh -> bbbbbbbb
            Tag.CreateForSeeding(Guid.Parse("bbbbbbbb-8888-8888-8888-aaaaaaaaaaaa"), "Fly Fishing", "Fishing with artificial flies", fishingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("bbbbbbbb-8888-8888-8888-bbbbbbbbbbbb"), "Deep Sea Fishing", "Fishing in ocean waters", fishingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("bbbbbbbb-8888-8888-8888-cccccccccccc"), "Ice Fishing", "Fishing through holes in ice", fishingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("bbbbbbbb-8888-8888-8888-dddddddddddd"), "Freshwater Fishing", "Fishing in lakes and rivers", fishingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("bbbbbbbb-8888-8888-8888-eeeeeeeeeeee"), "Kayak Fishing", "Fishing from a kayak", fishingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("bbbbbbbb-8888-8888-8888-ffffffffffff"), "Spearfishing", "Fishing with a spear", fishingGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("bbbbbbbb-8888-8888-8888-111111111111"), "Surf Fishing", "Fishing from shoreline", fishingGroupId).Value
        };

        builder.HasData(tags);
    }
}
