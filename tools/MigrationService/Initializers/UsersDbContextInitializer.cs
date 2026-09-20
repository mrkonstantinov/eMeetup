using eMeetup.Modules.Users.Domain.Tags;
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
            //var tags = await SeedTags();
            //await SeedEventsAsync(categories);
            await DbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }

    //private async Task<IReadOnlyList<UserTag>> SeedTags()
    //{
    //    if (await DbContext.Tags.AnyAsync())
    //        return [];

    //    var tags = new List<UserTag>
    //    {
    //    // Outdoor & Adventure
    //    UserTag.Create("Hiking", "Love exploring trails and mountains").Value,
    //    UserTag.Create("Camping", "Enjoy outdoor adventures and sleeping under the stars").Value,
    //    UserTag.Create("Beach Lover", "Perfect days spent by the ocean").Value,
    //    UserTag.Create("Travel", "Passionate about exploring new places").Value,
    //    UserTag.Create("Adventure", "Always seeking new experiences").Value,
    //    UserTag.Create("Nature", "Appreciate the great outdoors").Value,

    //    // Sports & Fitness
    //    UserTag.Create("Fitness", "Regular gym-goer or workout enthusiast").Value,
    //    UserTag.Create("Yoga", "Practice yoga and mindfulness").Value,
    //    UserTag.Create("Running", "Love jogging or marathon training").Value,
    //    UserTag.Create("Cycling", "Enjoy biking adventures").Value,
    //    UserTag.Create("Swimming", "Water sports and swimming").Value,
    //    UserTag.Create("Dancing", "Love to dance any chance I get").Value,

    //    // Creative & Arts
    //    UserTag.Create("Photography", "Capturing moments through lens").Value,
    //    UserTag.Create("Music", "Live for good music and concerts").Value,
    //    UserTag.Create("Art", "Appreciate museums and galleries").Value,
    //    UserTag.Create("Writing", "Creative writing or journaling").Value,
    //    UserTag.Create("Cooking", "Love experimenting in the kitchen").Value,
    //    UserTag.Create("Baking", "Enjoy making delicious treats").Value,

    //    // Entertainment
    //    UserTag.Create("Movies", "Film buff and cinema lover").Value,
    //    UserTag.Create("Netflix", "Enjoy cozy binge-watching nights").Value,
    //    UserTag.Create("Gaming", "Video games and board games").Value,
    //    UserTag.Create("Reading", "Bookworm and literature lover").Value,
    //    UserTag.Create("Theater", "Enjoy plays and performances").Value,
    //    UserTag.Create("Concerts", "Live music enthusiast").Value,

    //    // Social & Lifestyle
    //    UserTag.Create("Foodie", "Always trying new restaurants").Value,
    //    UserTag.Create("Wine Tasting", "Appreciate fine wines").Value,
    //    UserTag.Create("Coffee Lover", "Can't start the day without coffee").Value,
    //    UserTag.Create("Craft Beer", "Enjoy exploring local breweries").Value,
    //    UserTag.Create("Brunch", "Weekend brunch enthusiast").Value,
    //    UserTag.Create("Volunteering", "Giving back to the community").Value,

    //    // Intellectual
    //    UserTag.Create("Tech", "Technology and innovation enthusiast").Value,
    //    UserTag.Create("Science", "Fascinated by how things work").Value,
    //    UserTag.Create("History", "Love learning about the past").Value,
    //    UserTag.Create("Politics", "Stay informed and engaged").Value,
    //    UserTag.Create("Philosophy", "Deep conversations about life").Value,
    //    UserTag.Create("Learning", "Always seeking knowledge").Value,

    //    // Relaxation & Home
    //    UserTag.Create("Meditation", "Practice mindfulness and meditation").Value,
    //    UserTag.Create("Gardening", "Love growing plants and flowers").Value,
    //    UserTag.Create("DIY", "Do-it-yourself projects").Value,
    //    UserTag.Create("Home Decor", "Enjoy creating beautiful spaces").Value,
    //    UserTag.Create("Pets", "Animal lover and pet owner").Value,
    //    UserTag.Create("Sustainability", "Eco-conscious lifestyle").Value,

    //    // Specific Activities
    //    UserTag.Create("Surfing", "Ride the waves").Value,
    //    UserTag.Create("Snowboarding", "Winter sports enthusiast").Value,
    //    UserTag.Create("Rock Climbing", "Love climbing challenges").Value,
    //    UserTag.Create("Fishing", "Relaxing days by the water").Value,
    //    UserTag.Create("Golf", "Enjoy a round of golf").Value,
    //    UserTag.Create("Tennis", "Regular tennis player").Value,

    //    // Personality & Values
    //    UserTag.Create("Ambitious", "Driven and goal-oriented").Value,
    //    UserTag.Create("Family-Oriented", "Family is important to me").Value,
    //    UserTag.Create("Spiritual", "Value spiritual growth").Value,
    //    UserTag.Create("Feminist", "Believe in gender equality").Value,
    //    UserTag.Create("Minimalist", "Prefer simple living").Value,
    //    UserTag.Create("Entrepreneur", "Business-minded and innovative").Value
    //    };

    //    DbContext.Tags.AddRange(tags);

    //    return tags;
    //}
}
