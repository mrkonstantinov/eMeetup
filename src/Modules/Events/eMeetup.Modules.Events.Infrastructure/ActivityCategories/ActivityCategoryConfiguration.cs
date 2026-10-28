using eMeetup.Modules.Events.Domain.ActivityCategories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Modules.Events.Infrastructure.ActivityCategories;

internal sealed class ActivityCategoryConfiguration : IEntityTypeConfiguration<ActivityCategory>
{
    public void Configure(EntityTypeBuilder<ActivityCategory> builder)
    {
        builder.ToTable("activity_categories", "events");

        // === PRIMARY KEY ===
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd(); // int identity

        // === PROPERTIES ===
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("name");

        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("slug");

        builder.Property(c => c.Description)
            .HasMaxLength(500)
            .HasColumnName("description");

        builder.Property(c => c.Icon)
            .HasMaxLength(50)
            .HasColumnName("icon");

        builder.Property(c => c.Color)
            .HasMaxLength(7)
            .HasColumnName("color");

        builder.Property(c => c.ParentId)
            .HasColumnName("parent_id");

        builder.Property(c => c.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("display_order");

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasSentinel(ActivityCategoryStatus.Active)
            .HasDefaultValue(ActivityCategoryStatus.Active)
            .HasColumnName("status");

        builder.Property(c => c.IsSystem)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnName("is_system");

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        // === RELATIONSHIPS ===
        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // === INDEXES ===
        builder.HasIndex(c => c.Name)
            .HasDatabaseName("ix_activity_categories_name");

        builder.HasIndex(c => c.Slug)
            .IsUnique()
            .HasDatabaseName("ix_activity_categories_slug");

        builder.HasIndex(c => c.ParentId)
            .HasDatabaseName("ix_activity_categories_parent_id");

        builder.HasIndex(c => c.Status)
            .HasDatabaseName("ix_activity_categories_status");

        builder.HasIndex(c => c.DisplayOrder)
            .HasDatabaseName("ix_activity_categories_display_order");

        builder.HasIndex(c => new { c.Status, c.DisplayOrder })
            .HasDatabaseName("ix_activity_categories_status_display_order");

        // === SEED DATA ===
        builder.HasData(GetSeedCategories());
    }

    private static ActivityCategory[] GetSeedCategories()
    {
        var now = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        return new[]
        {
            // ============================================================
            // === КОРНЕВЫЕ КАТЕГОРИИ (Level 1) ===
            // ============================================================

            CreateSeed(
                id: 1,
                name: "Спорт",
                description: "Спортивные активности: бег, футбол, теннис и другие",
                icon: "⚽",
                color: "#FF6B6B",
                parentId: null,
                displayOrder: 1,
                createdAt: now),

            CreateSeed(
                id: 2,
                name: "Активный отдых",
                description: "Походы, велотуры, скалолазание и другие приключения",
                icon: "⛰️",
                color: "#4ECDC4",
                parentId: null,
                displayOrder: 2,
                createdAt: now),

            CreateSeed(
                id: 3,
                name: "Зимние активности",
                description: "Лыжи, сноуборд, коньки и тюбинг",
                icon: "❄️",
                color: "#45B7D1",
                parentId: null,
                displayOrder: 3,
                createdAt: now),

            CreateSeed(
                id: 4,
                name: "Водные приключения",
                description: "Пакрафтинг, САП, каякинг и другие водные походы",
                icon: "🚣",
                color: "#96CEB4",
                parentId: null,
                displayOrder: 4,
                createdAt: now),

            CreateSeed(
                id: 5,
                name: "Культура",
                description: "Театры, музеи, выставки, концерты",
                icon: "🎭",
                color: "#FFEAA7",
                parentId: null,
                displayOrder: 5,
                createdAt: now),

            CreateSeed(
                id: 6,
                name: "Музыка",
                description: "Концерты, джем-сейшны, музыкальные фестивали",
                icon: "🎵",
                color: "#DDA0DD",
                parentId: null,
                displayOrder: 6,
                createdAt: now),

            CreateSeed(
                id: 7,
                name: "Еда и напитки",
                description: "Рестораны, кафе, кулинарные мастер-классы",
                icon: "🍕",
                color: "#FF8A5C",
                parentId: null,
                displayOrder: 7,
                createdAt: now),

            CreateSeed(
                id: 8,
                name: "Образование",
                description: "Лекции, семинары, воркшопы, курсы",
                icon: "📚",
                color: "#A29BFE",
                parentId: null,
                displayOrder: 8,
                createdAt: now),

            CreateSeed(
                id: 9,
                name: "Бизнес",
                description: "Нетворкинг, стартапы, бизнес-встречи",
                icon: "💼",
                color: "#FD79A8",
                parentId: null,
                displayOrder: 9,
                createdAt: now),

            CreateSeed(
                id: 10,
                name: "Игры",
                description: "Настольные игры, киберспорт, квизы",
                icon: "🎮",
                color: "#00CEC9",
                parentId: null,
                displayOrder: 10,
                createdAt: now),

            CreateSeed(
                id: 11,
                name: "Путешествия",
                description: "Городские поездки, экскурсии, автопутешествия",
                icon: "✈️",
                color: "#FDCB6E",
                parentId: null,
                displayOrder: 11,
                createdAt: now),

            CreateSeed(
                id: 12,
                name: "Здоровье и велнес",
                description: "Йога, медитация, фитнес, здоровое питание",
                icon: "🧘",
                color: "#E17055",
                parentId: null,
                displayOrder: 12,
                createdAt: now),

            CreateSeed(
                id: 13,
                name: "Волонтерство",
                description: "Благотворительность, помощь животным, экология",
                icon: "🤲",
                color: "#6C5CE7",
                parentId: null,
                displayOrder: 13,
                createdAt: now),

            CreateSeed(
                id: 14,
                name: "Кино и сериалы",
                description: "Совместные походы в кино, обсуждения",
                icon: "🎬",
                color: "#00B894",
                parentId: null,
                displayOrder: 14,
                createdAt: now),

            CreateSeed(
                id: 15,
                name: "Фотография",
                description: "Фотосессии, фотопрогулки, обучение",
                icon: "📷",
                color: "#0984E3",
                parentId: null,
                displayOrder: 15,
                createdAt: now),

            // ============================================================
            // === ДОЧЕРНИЕ КАТЕГОРИИ (Level 2) ===
            // ============================================================

            // Спорт (1)
            CreateSeed(101, "Бег", "Бег, марафоны, забеги", "🏃", "#FF6B6B", 1, 1, now),
            CreateSeed(102, "Футбол", "Футбол, мини-футбол", "⚽", "#FF6B6B", 1, 2, now),
            CreateSeed(103, "Баскетбол", "Баскетбол, стритбол", "🏀", "#FF6B6B", 1, 3, now),
            CreateSeed(104, "Волейбол", "Волейбол, пляжный волейбол", "🏐", "#FF6B6B", 1, 4, now),
            CreateSeed(105, "Теннис", "Теннис, настольный теннис", "🎾", "#FF6B6B", 1, 5, now),
            CreateSeed(106, "Велоспорт", "Велоспорт, велопрогулки", "🚴", "#FF6B6B", 1, 6, now),
            CreateSeed(107, "Плавание", "Плавание, аквааэробика", "🏊", "#FF6B6B", 1, 7, now),
            CreateSeed(108, "Фитнес", "Тренажерный зал, групповые занятия", "💪", "#FF6B6B", 1, 8, now),

            // Активный отдых (2)
            CreateSeed(201, "Походы", "Пешие походы, треккинг", "🥾", "#4ECDC4", 2, 1, now),
            CreateSeed(202, "Кемпинг", "Кемпинг, палаточные лагеря", "🏕️", "#4ECDC4", 2, 2, now),
            CreateSeed(203, "Скалолазание", "Скалолазание, болдеринг", "🧗", "#4ECDC4", 2, 3, now),
            CreateSeed(204, "Пикник", "Пикники на природе", "🧺", "#4ECDC4", 2, 4, now),

            // Зимние (3)
            CreateSeed(301, "Лыжи", "Горные и беговые лыжи", "⛷️", "#45B7D1", 3, 1, now),
            CreateSeed(302, "Сноуборд", "Сноуборд", "🏂", "#45B7D1", 3, 2, now),
            CreateSeed(303, "Коньки", "Коньки, хоккей", "⛸️", "#45B7D1", 3, 3, now),
            CreateSeed(304, "Тюбинг", "Тюбинг, снежные горки", "🛷", "#45B7D1", 3, 4, now),

            // Водные (4)
            CreateSeed(401, "Каякинг", "Каякинг, сплавы", "🚣", "#96CEB4", 4, 1, now),
            CreateSeed(402, "САП", "Сапбординг", "🏄", "#96CEB4", 4, 2, now),
            CreateSeed(403, "Серфинг", "Серфинг", "🌊", "#96CEB4", 4, 3, now),
            CreateSeed(404, "Дайвинг", "Дайвинг, снорклинг", "🤿", "#96CEB4", 4, 4, now),

            // Культура (5)
            CreateSeed(501, "Театр", "Театры, спектакли", "🎭", "#FFEAA7", 5, 1, now),
            CreateSeed(502, "Музеи", "Музеи, выставки", "🏛️", "#FFEAA7", 5, 2, now),
            CreateSeed(503, "Экскурсии", "Экскурсии по городу", "🚶", "#FFEAA7", 5, 3, now),
            CreateSeed(504, "Кино", "Кинотеатры, кинопоказы", "🎬", "#FFEAA7", 5, 4, now),

            // Музыка (6)
            CreateSeed(601, "Концерты", "Концерты, живые выступления", "🎤", "#DDA0DD", 6, 1, now),
            CreateSeed(602, "Джем-сейшны", "Джем-сейшны, музыкальные встречи", "🎸", "#DDA0DD", 6, 2, now),
            CreateSeed(603, "Фестивали", "Музыкальные фестивали", "🎪", "#DDA0DD", 6, 3, now),

            // Еда (7)
            CreateSeed(701, "Рестораны", "Походы в рестораны", "🍽️", "#FF8A5C", 7, 1, now),
            CreateSeed(702, "Кафе", "Кофе, десерты", "☕", "#FF8A5C", 7, 2, now),
            CreateSeed(703, "Кулинария", "Кулинарные мастер-классы", "🍳", "#FF8A5C", 7, 3, now),
            CreateSeed(704, "Дегустации", "Винные, пивные дегустации", "🍷", "#FF8A5C", 7, 4, now),
            CreateSeed(705, "Барбекю", "Барбекю, гриль", "🔥", "#FF8A5C", 7, 5, now),

            // Образование (8)
            CreateSeed(801, "Лекции", "Лекции, семинары", "📖", "#A29BFE", 8, 1, now),
            CreateSeed(802, "Воркшопы", "Практические воркшопы", "🛠️", "#A29BFE", 8, 2, now),
            CreateSeed(803, "Языки", "Языковые встречи, разговорные клубы", "🗣️", "#A29BFE", 8, 3, now),
            CreateSeed(804, "Книги", "Книжные клубы", "📚", "#A29BFE", 8, 4, now),

            // Бизнес (9)
            CreateSeed(901, "Нетворкинг", "Бизнес-встречи, нетворкинг", "🤝", "#FD79A8", 9, 1, now),
            CreateSeed(902, "Стартапы", "Встречи стартаперов", "🚀", "#FD79A8", 9, 2, now),
            CreateSeed(903, "Конференции", "Конференции, форумы", "📊", "#FD79A8", 9, 3, now),

            // Игры (10)
            CreateSeed(1001, "Настольные игры", "Настольные игры, мафия", "🎲", "#00CEC9", 10, 1, now),
            CreateSeed(1002, "Киберспорт", "Компьютерные игры", "🖥️", "#00CEC9", 10, 2, now),
            CreateSeed(1003, "Квизы", "Интеллектуальные квизы", "🧠", "#00CEC9", 10, 3, now),
            CreateSeed(1004, "Квесты", "Квест-комнаты", "🔍", "#00CEC9", 10, 4, now),

            // Путешествия (11)
            CreateSeed(1101, "Городские поездки", "Поездки по городам", "🏙️", "#FDCB6E", 11, 1, now),
            CreateSeed(1102, "Автопутешествия", "Автопутешествия, road trips", "🚗", "#FDCB6E", 11, 2, now),
            CreateSeed(1103, "Экотуризм", "Экотуризм, природа", "🌿", "#FDCB6E", 11, 3, now),

            // Здоровье (12)
            CreateSeed(1201, "Йога", "Йога, пилатес", "🧘‍♀️", "#E17055", 12, 1, now),
            CreateSeed(1202, "Медитация", "Медитация, дыхательные практики", "🕯️", "#E17055", 12, 2, now),
            CreateSeed(1203, "Спа", "Спа, велнес", "💆", "#E17055", 12, 3, now),

            // Волонтерство (13)
            CreateSeed(1301, "Помощь животным", "Приюты, помощь животным", "🐕", "#6C5CE7", 13, 1, now),
            CreateSeed(1302, "Экология", "Экологические акции", "♻️", "#6C5CE7", 13, 2, now),
            CreateSeed(1303, "Благотворительность", "Благотворительные мероприятия", "❤️", "#6C5CE7", 13, 3, now),

            // Кино (14)
            CreateSeed(1401, "Кинотеатры", "Походы в кино", "🍿", "#00B894", 14, 1, now),
            CreateSeed(1402, "Киноклубы", "Обсуждения фильмов", "🎞️", "#00B894", 14, 2, now),

            // Фотография (15)
            CreateSeed(1501, "Фотопрогулки", "Фотопрогулки, фотосессии", "🌅", "#0984E3", 15, 1, now),
            CreateSeed(1502, "Обучение", "Курсы фотографии", "📸", "#0984E3", 15, 2, now),
        };
    }

    private static ActivityCategory CreateSeed(
        int id,
        string name,
        string description,
        string icon,
        string color,
        int? parentId,
        int displayOrder,
        DateTimeOffset createdAt)
    {
        return ActivityCategory.CreateForSeeding(
            id: id,
            name: name,
            description: description,
            icon: icon,
            color: color,
            parentId: parentId,
            displayOrder: displayOrder,
            createdAt: createdAt);
    }
}
