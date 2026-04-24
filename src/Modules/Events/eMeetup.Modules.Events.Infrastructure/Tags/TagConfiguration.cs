using eMeetup.Modules.Events.Domain.TagGroups;
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
        var activeLifestyleGroupId = 1;
        var winterActivitiesGroupId = 2;
        var adventureOutdoorsGroupId = 3;
        var waterAdventuresGroupId = 4;
        var festivalsEventsGroupId = 5;
        var travelExplorationGroupId = 6;
        var cultureEntertainmentGroupId = 7;
        var chillHangoutGroupId = 8;
        var sportsEventsGroupId = 9;

        // Create tags using seeding factory method
        var tags = new List<Tag>
        {
            // Теги активного образа жизни
            Tag.CreateForSeeding(Guid.Parse("11111111-1111-1111-1111-aaaaaaaaaaaa"), "Бег", "Занятия бегом и пробежки", activeLifestyleGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("11111111-1111-1111-1111-bbbbbbbbbbbb"), "Плавание", "Тренировки в бассейне", activeLifestyleGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("11111111-1111-1111-1111-cccccccccccc"), "Велоспорт", "Поездки по городу и шоссе", activeLifestyleGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("11111111-1111-1111-1111-dddddddddddd"), "Тренажерный зал", "Фитнес и силовые тренировки", activeLifestyleGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("11111111-1111-1111-1111-eeeeeeeeeeee"), "Йога", "Занятия йогой и растяжка", activeLifestyleGroupId).Value,

            // Теги зимних активностей
            Tag.CreateForSeeding(Guid.Parse("22222222-2222-2222-2222-aaaaaaaaaaaa"), "Горные лыжи", "Катание на склонах", winterActivitiesGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("22222222-2222-2222-2222-bbbbbbbbbbbb"), "Беговые лыжи", "Скандинавская ходьба на лыжах по трассам", winterActivitiesGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("22222222-2222-2222-2222-cccccccccccc"), "Сноуборд", "Катание на склонах и в парках", winterActivitiesGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("22222222-2222-2222-2222-dddddddddddd"), "Катание на коньках", "Катание на ледовых катках", winterActivitiesGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("22222222-2222-2222-2222-eeeeeeeeeeee"), "Тюбинг", "Катание на ватрушках с горок", winterActivitiesGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("22222222-2222-2222-2222-ffffffffffff"), "Зимний поход", "Походы на снегоступах", winterActivitiesGroupId).Value,

            // Теги приключений и активного отдыха
            Tag.CreateForSeeding(Guid.Parse("33333333-3333-3333-3333-aaaaaaaaaaaa"), "Поход", "Дневные походы по тропам", adventureOutdoorsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("33333333-3333-3333-3333-bbbbbbbbbbbb"), "Велотур", "Многодневные велосипедные путешествия", adventureOutdoorsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("33333333-3333-3333-3333-cccccccccccc"), "Пикник", "Трапезы на природе", adventureOutdoorsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("33333333-3333-3333-3333-dddddddddddd"), "Кемпинг", "Ночёвки на природе", adventureOutdoorsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("33333333-3333-3333-3333-eeeeeeeeeeee"), "Трейлраннинг", "Бег по природным тропам", adventureOutdoorsGroupId).Value,

            // Теги водных приключений
            Tag.CreateForSeeding(Guid.Parse("44444444-4444-4444-4444-aaaaaaaaaaaa"), "Пакрафтинг", "Сплав на лёгких надувных лодках", waterAdventuresGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("44444444-4444-4444-4444-bbbbbbbbbbbb"), "САП-сёрфинг", "Катание на доске с веслом стоя", waterAdventuresGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("44444444-4444-4444-4444-cccccccccccc"), "Каякинг", "Сплав на каяках по рекам и озёрам", waterAdventuresGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("44444444-4444-4444-4444-dddddddddddd"), "Каноэ", "Путешествия на каноэ по спокойной воде", waterAdventuresGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("44444444-4444-4444-4444-eeeeeeeeeeee"), "Рафтинг", "Сплав по бурной воде", waterAdventuresGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("44444444-4444-4444-4444-ffffffffffff"), "Плавание на открытой воде", "Плавание в озёрах и морях", waterAdventuresGroupId).Value,

            // Теги фестивалей и мероприятий
            Tag.CreateForSeeding(Guid.Parse("55555555-5555-5555-5555-aaaaaaaaaaaa"), "Музыкальный фестиваль", "Фестивали живой музыки и концертов", festivalsEventsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("55555555-5555-5555-5555-bbbbbbbbbbbb"), "Гастрофестиваль", "Кулинарные мероприятия", festivalsEventsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("55555555-5555-5555-5555-cccccccccccc"), "Культурный фестиваль", "Традиционные праздники", festivalsEventsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("55555555-5555-5555-5555-dddddddddddd"), "Городской праздник", "Местные ярмарки и гуляния", festivalsEventsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("55555555-5555-5555-5555-eeeeeeeeeeee"), "Пивной фестиваль", "Дегустации крафтового пива", festivalsEventsGroupId).Value,

            // Теги путешествий
            Tag.CreateForSeeding(Guid.Parse("66666666-6666-6666-6666-aaaaaaaaaaaa"), "Городская поездка", "Осмотр достопримечательностей", travelExplorationGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("66666666-6666-6666-6666-bbbbbbbbbbbb"), "Гастротур", "Кулинарные и винные туры", travelExplorationGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("66666666-6666-6666-6666-cccccccccccc"), "Автопутешествие", "Путешествия на машине", travelExplorationGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("66666666-6666-6666-6666-dddddddddddd"), "Культурный тур", "Экскурсии по музеям и архитектуре", travelExplorationGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("66666666-6666-6666-6666-eeeeeeeeeeee"), "Уикенд за городом", "Короткие поездки за город", travelExplorationGroupId).Value,

            // Теги культуры и развлечений
            Tag.CreateForSeeding(Guid.Parse("77777777-7777-7777-7777-aaaaaaaaaaaa"), "Кино", "Фильмы и кинопоказы", cultureEntertainmentGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("77777777-7777-7777-7777-bbbbbbbbbbbb"), "Театр", "Спектакли и театральные постановки", cultureEntertainmentGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("77777777-7777-7777-7777-cccccccccccc"), "Концерт", "Концерты живой музыки", cultureEntertainmentGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("77777777-7777-7777-7777-dddddddddddd"), "Рок-концерт", "Рок и метал концерты", cultureEntertainmentGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("77777777-7777-7777-7777-eeeeeeeeeeee"), "Выставка искусств", "Художественные галереи и выставки", cultureEntertainmentGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("77777777-7777-7777-7777-ffffffffffff"), "Стендап", "Юмористические выступления", cultureEntertainmentGroupId).Value,

            // Теги отдыха и встреч
            Tag.CreateForSeeding(Guid.Parse("88888888-8888-8888-8888-aaaaaaaaaaaa"), "Паб", "Встречи в барах и пабах", chillHangoutGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("88888888-8888-8888-8888-bbbbbbbbbbbb"), "Веранда", "Летние кафе и террасы", chillHangoutGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("88888888-8888-8888-8888-cccccccccccc"), "Кофе-дейт", "Неформальные встречи за кофе", chillHangoutGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("88888888-8888-8888-8888-dddddddddddd"), "Настольные игры", "Вечера настольных игр", chillHangoutGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("88888888-8888-8888-8888-eeeeeeeeeeee"), "Пикник в парке", "Неспешные встречи в парке", chillHangoutGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("88888888-8888-8888-8888-ffffffffffff"), "Отдых в гамаке", "Расслабленный летний отдых в гамаке на природе", chillHangoutGroupId).Value,

            // Теги спортивных событий
            Tag.CreateForSeeding(Guid.Parse("99999999-9999-9999-9999-aaaaaaaaaaaa"), "Триатлон", "Соревнования по плаванию, велоспорту и бегу", sportsEventsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("99999999-9999-9999-9999-bbbbbbbbbbbb"), "Марафон", "Участие в марафонах в других городах", sportsEventsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("99999999-9999-9999-9999-cccccccccccc"), "Гонка с препятствиями", "Забеги с препятствиями", sportsEventsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("99999999-9999-9999-9999-dddddddddddd"), "Велосипедная гонка", "Соревновательные велозаезды", sportsEventsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("99999999-9999-9999-9999-eeeeeeeeeeee"), "Соревнования по плаванию", "Заплывы на открытой воде или в бассейне", sportsEventsGroupId).Value,
            Tag.CreateForSeeding(Guid.Parse("99999999-9999-9999-9999-ffffffffffff"), "Трейловый забег", "Соревнования по бегу по бездорожью", sportsEventsGroupId).Value,
        };

        builder.HasData(tags);
    }
}
