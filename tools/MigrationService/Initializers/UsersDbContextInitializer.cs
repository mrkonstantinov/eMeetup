using eMeetup.Modules.Users.Domain.Users;
using eMeetup.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace MigrationService.Initializers;

internal class UsersDbContextInitializer : DbContextInitializerBase<UsersDbContext>
{
    public UsersDbContextInitializer(UsersDbContext dbContext) : base(dbContext)
    {
    }

    public async Task SeedDataAsync(CancellationToken cancellationToken)
    {
        var strategy = DbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Seed the database
            await using var transaction = await DbContext.Database.BeginTransactionAsync(cancellationToken);
            var tags = await SeedTags();
            //await SeedEventsAsync(categories);
            await DbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }

    private async Task<IReadOnlyList<Tag>> SeedTags()
    {
        if (await DbContext.Tags.AnyAsync())
            return [];

        var tags = new List<Tag>
        {
        // Outdoor & Adventure
        Tag.Create("Hiking", "Love exploring trails and mountains").Value,
        Tag.Create("Camping", "Enjoy outdoor adventures and sleeping under the stars").Value,
        Tag.Create("Beach Lover", "Perfect days spent by the ocean").Value,
        Tag.Create("Travel", "Passionate about exploring new places").Value,
        Tag.Create("Adventure", "Always seeking new experiences").Value,
        Tag.Create("Nature", "Appreciate the great outdoors").Value,

        // Sports & Fitness
        Tag.Create("Fitness", "Regular gym-goer or workout enthusiast").Value,
        Tag.Create("Yoga", "Practice yoga and mindfulness").Value,
        Tag.Create("Running", "Love jogging or marathon training").Value,
        Tag.Create("Cycling", "Enjoy biking adventures").Value,
        Tag.Create("Swimming", "Water sports and swimming").Value,
        Tag.Create("Dancing", "Love to dance any chance I get").Value,

        // Creative & Arts
        Tag.Create("Photography", "Capturing moments through lens").Value,
        Tag.Create("Music", "Live for good music and concerts").Value,
        Tag.Create("Art", "Appreciate museums and galleries").Value,
        Tag.Create("Writing", "Creative writing or journaling").Value,
        Tag.Create("Cooking", "Love experimenting in the kitchen").Value,
        Tag.Create("Baking", "Enjoy making delicious treats").Value,

        // Entertainment
        Tag.Create("Movies", "Film buff and cinema lover").Value,
        Tag.Create("Netflix", "Enjoy cozy binge-watching nights").Value,
        Tag.Create("Gaming", "Video games and board games").Value,
        Tag.Create("Reading", "Bookworm and literature lover").Value,
        Tag.Create("Theater", "Enjoy plays and performances").Value,
        Tag.Create("Concerts", "Live music enthusiast").Value,

        // Social & Lifestyle
        Tag.Create("Foodie", "Always trying new restaurants").Value,
        Tag.Create("Wine Tasting", "Appreciate fine wines").Value,
        Tag.Create("Coffee Lover", "Can't start the day without coffee").Value,
        Tag.Create("Craft Beer", "Enjoy exploring local breweries").Value,
        Tag.Create("Brunch", "Weekend brunch enthusiast").Value,
        Tag.Create("Volunteering", "Giving back to the community").Value,

        // Intellectual
        Tag.Create("Tech", "Technology and innovation enthusiast").Value,
        Tag.Create("Science", "Fascinated by how things work").Value,
        Tag.Create("History", "Love learning about the past").Value,
        Tag.Create("Politics", "Stay informed and engaged").Value,
        Tag.Create("Philosophy", "Deep conversations about life").Value,
        Tag.Create("Learning", "Always seeking knowledge").Value,

        // Relaxation & Home
        Tag.Create("Meditation", "Practice mindfulness and meditation").Value,
        Tag.Create("Gardening", "Love growing plants and flowers").Value,
        Tag.Create("DIY", "Do-it-yourself projects").Value,
        Tag.Create("Home Decor", "Enjoy creating beautiful spaces").Value,
        Tag.Create("Pets", "Animal lover and pet owner").Value,
        Tag.Create("Sustainability", "Eco-conscious lifestyle").Value,

        // Specific Activities
        Tag.Create("Surfing", "Ride the waves").Value,
        Tag.Create("Snowboarding", "Winter sports enthusiast").Value,
        Tag.Create("Rock Climbing", "Love climbing challenges").Value,
        Tag.Create("Fishing", "Relaxing days by the water").Value,
        Tag.Create("Golf", "Enjoy a round of golf").Value,
        Tag.Create("Tennis", "Regular tennis player").Value,

        // Personality & Values
        Tag.Create("Ambitious", "Driven and goal-oriented").Value,
        Tag.Create("Family-Oriented", "Family is important to me").Value,
        Tag.Create("Spiritual", "Value spiritual growth").Value,
        Tag.Create("Feminist", "Believe in gender equality").Value,
        Tag.Create("Minimalist", "Prefer simple living").Value,
        Tag.Create("Entrepreneur", "Business-minded and innovative").Value
        };

        DbContext.Tags.AddRange(tags);

        return tags;
    }
}
