using eMeetup.Modules.Users.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Modules.Users.Infrastructure.Tags;

public class TagGroupConfiguration : IEntityTypeConfiguration<TagGroup>
{
    public void Configure(EntityTypeBuilder<TagGroup> builder)
    {
        builder.ToTable("tag_groups");

        // === PRIMARY KEY ===
        builder.HasKey(tg => tg.Id);
        builder.Property(tg => tg.Id)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()");

        // === PROPERTIES ===
        builder.Property(tg => tg.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(tg => tg.Description)
            .HasMaxLength(500)
            .HasDefaultValue(null);

        builder.Property(tg => tg.Icon)
            .HasMaxLength(50)
            .HasDefaultValue(null);

        builder.Property(tg => tg.Color)
            .HasMaxLength(7)
            .HasDefaultValue(null);

        builder.Property(tg => tg.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(tg => tg.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(tg => tg.IsSystem)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(tg => tg.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(tg => tg.UpdatedAt)
            .HasDefaultValue(null);

        builder.Property(tg => tg.DeactivatedAt)
            .HasDefaultValue(null);

        // === INDEXES ===
        builder.HasIndex(tg => tg.Name)
            .IsUnique()
            .HasDatabaseName("ix_tag_groups_name");

        builder.HasIndex(tg => tg.IsActive)
            .HasDatabaseName("ix_tag_groups_is_active");

        builder.HasIndex(tg => tg.DisplayOrder)
            .HasDatabaseName("ix_tag_groups_display_order");

        builder.HasIndex(tg => tg.IsSystem)
            .HasDatabaseName("ix_tag_groups_is_system");

        builder.HasIndex(tg => new { tg.IsActive, tg.DisplayOrder })
            .HasDatabaseName("ix_tag_groups_active_order");

        // === RELATIONSHIPS ===
        builder.HasMany(tg => tg.Tags)
            .WithOne(t => t.TagGroup)
            .HasForeignKey(t => t.TagGroupId)
            .OnDelete(DeleteBehavior.SetNull);

        // === SEED DATA ===
        var seedGroups = GetSeedGroups();
        builder.HasData(seedGroups);
    }

    private static TagGroup[] GetSeedGroups()
    {
        return new[]
        {
            // Активный образ жизни
            CreateSystemGroup(
                id: "11111111-1111-1111-1111-111111111111",
                name: "Активный образ жизни",
                description: "Бег, плавание, велоспорт и другие спортивные активности",
                icon: "🏃",
                color: "#FF6B6B",
                displayOrder: 1
            ),
            
            // Зимние активности
            CreateSystemGroup(
                id: "22222222-2222-2222-2222-222222222222",
                name: "Зимние активности",
                description: "Лыжи, сноуборд, коньки и тюбинг",
                icon: "❄️",
                color: "#4ECDC4",
                displayOrder: 2
            ),
            
            // Приключения
            CreateSystemGroup(
                id: "33333333-3333-3333-3333-333333333333",
                name: "Приключения",
                description: "Походы, велотуры и пикники на природе",
                icon: "⛰️",
                color: "#45B7D1",
                displayOrder: 3
            ),
            
            // Водные приключения
            CreateSystemGroup(
                id: "44444444-4444-4444-4444-444444444444",
                name: "Водные приключения",
                description: "Пакрафтинг, САП, каякинг и другие водные походы",
                icon: "🚣",
                color: "#96CEB4",
                displayOrder: 4
            ),
            
            // Фестивали и мероприятия
            CreateSystemGroup(
                id: "55555555-5555-5555-5555-555555555555",
                name: "Фестивали и мероприятия",
                description: "Музыкальные, гастрономические и культурные фестивали",
                icon: "🎪",
                color: "#FFEAA7",
                displayOrder: 5
            ),
            
            // Путешествия
            CreateSystemGroup(
                id: "66666666-6666-6666-6666-666666666666",
                name: "Путешествия",
                description: "Городские поездки, экскурсии, гастротуры и автопутешествия",
                icon: "✈️",
                color: "#A29BFE",
                displayOrder: 6
            ),
            
            // Культура и развлечения
            CreateSystemGroup(
                id: "77777777-7777-7777-7777-777777777777",
                name: "Культура и развлечения",
                description: "Кино, театр, концерты и рок-шоу",
                icon: "🎭",
                color: "#FD79A8",
                displayOrder: 7
            ),
            
            // Отдых и тусовки
            CreateSystemGroup(
                id: "88888888-8888-8888-8888-888888888888",
                name: "Отдых и тусовки",
                description: "Встречи в пабах, летние веранды и приятное времяпрепровождение",
                icon: "😎",
                color: "#FDCB6E",
                displayOrder: 8
            ),
            
            // Спортивные события
            CreateSystemGroup(
                id: "99999999-9999-9999-9999-999999999999",
                name: "Спортивные события",
                description: "Марафоны, триатлоны, соревнования и забеги в других городах",
                icon: "🏆",
                color: "#E17055",
                displayOrder: 9
            ),
            
            // Еда и кулинария
            CreateSystemGroup(
                id: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
                name: "Еда и кулинария",
                description: "Рестораны, кафе, кулинарные мастер-классы и гастрономические туры",
                icon: "🍕",
                color: "#FF8A5C",
                displayOrder: 10
            ),
            
            // Музыка
            CreateSystemGroup(
                id: "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
                name: "Музыка",
                description: "Концерты, джем-сейшны, музыкальные фестивали",
                icon: "🎵",
                color: "#6C5CE7",
                displayOrder: 11
            ),
            
            // Искусство
            CreateSystemGroup(
                id: "cccccccc-cccc-cccc-cccc-cccccccccccc",
                name: "Искусство",
                description: "Выставки, галереи, арт-вечеринки",
                icon: "🎨",
                color: "#00CEC9",
                displayOrder: 12
            ),
            
            // Спорт
            CreateSystemGroup(
                id: "dddddddd-dddd-dddd-dddd-dddddddddddd",
                name: "Спорт",
                description: "Теннис, баскетбол, футбол и другие виды спорта",
                icon: "⚽",
                color: "#00B894",
                displayOrder: 13
            ),
            
            // Киберспорт
            CreateSystemGroup(
                id: "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
                name: "Киберспорт и игры",
                description: "Dota 2, CS:GO, настольные игры",
                icon: "🎮",
                color: "#0984E3",
                displayOrder: 14
            ),
            
            // Образование
            CreateSystemGroup(
                id: "ffffffff-ffff-ffff-ffff-ffffffffffff",
                name: "Образование",
                description: "Лекции, семинары, воркшопы, языковые курсы",
                icon: "📚",
                color: "#F39C12",
                displayOrder: 15
            ),
            
            // Бизнес
            CreateSystemGroup(
                id: "10101010-1010-1010-1010-101010101010",
                name: "Бизнес и карьера",
                description: "Нетворкинг, стартапы, бизнес-встречи",
                icon: "💼",
                color: "#2D3436",
                displayOrder: 16
            ),
            
            // Природа
            CreateSystemGroup(
                id: "12121212-1212-1212-1212-121212121212",
                name: "Природа",
                description: "Экотуризм, заповедники, наблюдение за птицами",
                icon: "🌿",
                color: "#27AE60",
                displayOrder: 17
            ),
            
            // Здоровье
            CreateSystemGroup(
                id: "13131313-1313-1313-1313-131313131313",
                name: "Здоровье и велнес",
                description: "Йога, медитация, фитнес, здоровое питание",
                icon: "🧘",
                color: "#A29BFE",
                displayOrder: 18
            ),
            
            // Волонтерство
            CreateSystemGroup(
                id: "14141414-1414-1414-1414-141414141414",
                name: "Волонтерство",
                description: "Благотворительность, помощь животным, экологические акции",
                icon: "🤲",
                color: "#E17055",
                displayOrder: 19
            ),
        };
    }

    private static TagGroup CreateSystemGroup(
        string id,
        string name,
        string description,
        string icon,
        string color,
        int displayOrder)
    {
        // Используем рефлексию или публичный метод для создания с ID
        var result = TagGroup.CreateSystem(
            name,
            description,
            icon,
            color,
            displayOrder);

        if (result.IsFailure)
            throw new InvalidOperationException($"Failed to create seed group: {result.Error.Description}");

        var group = result.Value;

        // Устанавливаем ID через рефлексию (т.к. setter private)
        typeof(TagGroup)
            .GetProperty(nameof(TagGroup.Id))?
            .SetValue(group, Guid.Parse(id));

        // Устанавливаем CreatedAt (чтобы не было конфликтов при миграции)
        typeof(TagGroup)
            .GetProperty(nameof(TagGroup.CreatedAt))?
            .SetValue(group, DateTime.UtcNow);

        return group;
    }
}
