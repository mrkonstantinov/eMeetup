using eMeetup.Modules.Events.Domain.TagGroups;
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
            .UseIdentityColumn()
            .ValueGeneratedOnAdd();

        // Name
        builder.Property(tg => tg.Name)
            .IsRequired()
            .HasMaxLength(50);

        // Description
        builder.Property(tg => tg.Description)
            .HasMaxLength(200)
            .HasDefaultValue(string.Empty);

        // Icon
        builder.Property(tg => tg.PictureFileName)
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
    1,
    "Активный образ жизни",
    "Бег, плавание, велоспорт и другие спортивные активности",
    "🏃",
    1
).Value;

        var winterActivitiesGroup = TagGroup.CreateForSeeding(
            2,
            "Зимние активности",
            "Лыжи, сноуборд, коньки и тюбинг",
            "❄️",
            2
        ).Value;

        var adventureOutdoorsGroup = TagGroup.CreateForSeeding(
            3,
            "Приключения и активный отдых",
            "Походы, велотуры и пикники на природе",
            "⛰️",
            3
        ).Value;

        var waterAdventuresGroup = TagGroup.CreateForSeeding(
            4,
            "Водные приключения",
            "Пакрафтинг, САП, каякинг и другие водные походы",
            "🚣",
            4
        ).Value;

        var festivalsEventsGroup = TagGroup.CreateForSeeding(
            5,
            "Фестивали и мероприятия",
            "Музыкальные, гастрономические и культурные фестивали",
            "🎪",
            5
        ).Value;

        var travelExplorationGroup = TagGroup.CreateForSeeding(
            6,
            "Путешествия",
            "Городские поездки, экскурсии, гастротуры и автопутешествия",
            "✈️",
            6
        ).Value;

        var cultureEntertainmentGroup = TagGroup.CreateForSeeding(
            7,
            "Культура и развлечения",
            "Кино, театр, концерты и рок-шоу",
            "🎭",
            7
        ).Value;

        var chillHangoutGroup = TagGroup.CreateForSeeding(
            8,
            "Отдых и тусовки",
            "Встречи в пабах, летние веранды и приятное времяпрепровождение",
            "😎",
            8
        ).Value;

        var sportsEventsGroup = TagGroup.CreateForSeeding(
            9,
            "Спортивные события",
            "Марафоны, триатлоны, соревнования и забеги в других городах",
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
