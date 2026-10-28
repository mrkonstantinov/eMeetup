using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace eMeetup.Modules.Events.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "events");

            migrationBuilder.CreateTable(
                name: "activity_categories",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_categories", x => x.id);
                    table.ForeignKey(
                        name: "fk_activity_categories_activity_categories_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "events",
                        principalTable: "activity_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inbox_message_consumers",
                schema: "events",
                columns: table => new
                {
                    inbox_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_message_consumers", x => new { x.inbox_message_id, x.name });
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "jsonb", maxLength: 2000, nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_message_consumers",
                schema: "events",
                columns: table => new
                {
                    outbox_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_message_consumers", x => new { x.outbox_message_id, x.name });
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "jsonb", maxLength: 2000, nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.InsertData(
                schema: "events",
                table: "activity_categories",
                columns: new[] { "id", "color", "created_at", "description", "display_order", "icon", "is_system", "name", "parent_id", "slug", "status", "updated_at" },
                values: new object[,]
                {
                    { 1, "#FF6B6B", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Спортивные активности: бег, футбол, теннис и другие", 1, "⚽", true, "Спорт", null, "sport", 1, null },
                    { 2, "#4ECDC4", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Походы, велотуры, скалолазание и другие приключения", 2, "⛰️", true, "Активный отдых", null, "aktivnyi-otdyh", 1, null },
                    { 3, "#45B7D1", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Лыжи, сноуборд, коньки и тюбинг", 3, "❄️", true, "Зимние активности", null, "zimnie-aktivnosti", 1, null },
                    { 4, "#96CEB4", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Пакрафтинг, САП, каякинг и другие водные походы", 4, "🚣", true, "Водные приключения", null, "vodnye-priklyucheniya", 1, null },
                    { 5, "#FFEAA7", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Театры, музеи, выставки, концерты", 5, "🎭", true, "Культура", null, "kultura", 1, null },
                    { 6, "#DDA0DD", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Концерты, джем-сейшны, музыкальные фестивали", 6, "🎵", true, "Музыка", null, "muzyka", 1, null },
                    { 7, "#FF8A5C", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Рестораны, кафе, кулинарные мастер-классы", 7, "🍕", true, "Еда и напитки", null, "eda-i-napitki", 1, null },
                    { 8, "#A29BFE", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Лекции, семинары, воркшопы, курсы", 8, "📚", true, "Образование", null, "obrazovanie", 1, null },
                    { 9, "#FD79A8", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Нетворкинг, стартапы, бизнес-встречи", 9, "💼", true, "Бизнес", null, "biznes", 1, null },
                    { 10, "#00CEC9", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Настольные игры, киберспорт, квизы", 10, "🎮", true, "Игры", null, "igry", 1, null },
                    { 11, "#FDCB6E", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Городские поездки, экскурсии, автопутешествия", 11, "✈️", true, "Путешествия", null, "puteshestviya", 1, null },
                    { 12, "#E17055", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Йога, медитация, фитнес, здоровое питание", 12, "🧘", true, "Здоровье и велнес", null, "zdorove-i-velnes", 1, null },
                    { 13, "#6C5CE7", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Благотворительность, помощь животным, экология", 13, "🤲", true, "Волонтерство", null, "volonterstvo", 1, null },
                    { 14, "#00B894", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Совместные походы в кино, обсуждения", 14, "🎬", true, "Кино и сериалы", null, "kino-i-serialy", 1, null },
                    { 15, "#0984E3", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Фотосессии, фотопрогулки, обучение", 15, "📷", true, "Фотография", null, "fotografiya", 1, null },
                    { 101, "#FF6B6B", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Бег, марафоны, забеги", 1, "🏃", true, "Бег", 1, "beg", 1, null },
                    { 102, "#FF6B6B", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Футбол, мини-футбол", 2, "⚽", true, "Футбол", 1, "futbol", 1, null },
                    { 103, "#FF6B6B", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Баскетбол, стритбол", 3, "🏀", true, "Баскетбол", 1, "basketbol", 1, null },
                    { 104, "#FF6B6B", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Волейбол, пляжный волейбол", 4, "🏐", true, "Волейбол", 1, "voleibol", 1, null },
                    { 105, "#FF6B6B", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Теннис, настольный теннис", 5, "🎾", true, "Теннис", 1, "tennis", 1, null },
                    { 106, "#FF6B6B", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Велоспорт, велопрогулки", 6, "🚴", true, "Велоспорт", 1, "velosport", 1, null },
                    { 107, "#FF6B6B", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Плавание, аквааэробика", 7, "🏊", true, "Плавание", 1, "plavanie", 1, null },
                    { 108, "#FF6B6B", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Тренажерный зал, групповые занятия", 8, "💪", true, "Фитнес", 1, "fitnes", 1, null },
                    { 201, "#4ECDC4", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Пешие походы, треккинг", 1, "🥾", true, "Походы", 2, "pohody", 1, null },
                    { 202, "#4ECDC4", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Кемпинг, палаточные лагеря", 2, "🏕️", true, "Кемпинг", 2, "kemping", 1, null },
                    { 203, "#4ECDC4", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Скалолазание, болдеринг", 3, "🧗", true, "Скалолазание", 2, "skalolazanie", 1, null },
                    { 204, "#4ECDC4", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Пикники на природе", 4, "🧺", true, "Пикник", 2, "piknik", 1, null },
                    { 301, "#45B7D1", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Горные и беговые лыжи", 1, "⛷️", true, "Лыжи", 3, "lyzhi", 1, null },
                    { 302, "#45B7D1", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Сноуборд", 2, "🏂", true, "Сноуборд", 3, "snoubord", 1, null },
                    { 303, "#45B7D1", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Коньки, хоккей", 3, "⛸️", true, "Коньки", 3, "konki", 1, null },
                    { 304, "#45B7D1", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Тюбинг, снежные горки", 4, "🛷", true, "Тюбинг", 3, "tyubing", 1, null },
                    { 401, "#96CEB4", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Каякинг, сплавы", 1, "🚣", true, "Каякинг", 4, "kayaking", 1, null },
                    { 402, "#96CEB4", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Сапбординг", 2, "🏄", true, "САП", 4, "sap", 1, null },
                    { 403, "#96CEB4", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Серфинг", 3, "🌊", true, "Серфинг", 4, "serfing", 1, null },
                    { 404, "#96CEB4", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Дайвинг, снорклинг", 4, "🤿", true, "Дайвинг", 4, "daiving", 1, null },
                    { 501, "#FFEAA7", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Театры, спектакли", 1, "🎭", true, "Театр", 5, "teatr", 1, null },
                    { 502, "#FFEAA7", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Музеи, выставки", 2, "🏛️", true, "Музеи", 5, "muzei", 1, null },
                    { 503, "#FFEAA7", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Экскурсии по городу", 3, "🚶", true, "Экскурсии", 5, "ekskursii", 1, null },
                    { 504, "#FFEAA7", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Кинотеатры, кинопоказы", 4, "🎬", true, "Кино", 5, "kino", 1, null },
                    { 601, "#DDA0DD", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Концерты, живые выступления", 1, "🎤", true, "Концерты", 6, "kontserty", 1, null },
                    { 602, "#DDA0DD", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Джем-сейшны, музыкальные встречи", 2, "🎸", true, "Джем-сейшны", 6, "dzhem-seishny", 1, null },
                    { 603, "#DDA0DD", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Музыкальные фестивали", 3, "🎪", true, "Фестивали", 6, "festivali", 1, null },
                    { 701, "#FF8A5C", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Походы в рестораны", 1, "🍽️", true, "Рестораны", 7, "restorany", 1, null },
                    { 702, "#FF8A5C", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Кофе, десерты", 2, "☕", true, "Кафе", 7, "kafe", 1, null },
                    { 703, "#FF8A5C", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Кулинарные мастер-классы", 3, "🍳", true, "Кулинария", 7, "kulinariya", 1, null },
                    { 704, "#FF8A5C", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Винные, пивные дегустации", 4, "🍷", true, "Дегустации", 7, "degustatsii", 1, null },
                    { 705, "#FF8A5C", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Барбекю, гриль", 5, "🔥", true, "Барбекю", 7, "barbekyu", 1, null },
                    { 801, "#A29BFE", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Лекции, семинары", 1, "📖", true, "Лекции", 8, "lektsii", 1, null },
                    { 802, "#A29BFE", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Практические воркшопы", 2, "🛠️", true, "Воркшопы", 8, "vorkshopy", 1, null },
                    { 803, "#A29BFE", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Языковые встречи, разговорные клубы", 3, "🗣️", true, "Языки", 8, "yazyki", 1, null },
                    { 804, "#A29BFE", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Книжные клубы", 4, "📚", true, "Книги", 8, "knigi", 1, null },
                    { 901, "#FD79A8", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Бизнес-встречи, нетворкинг", 1, "🤝", true, "Нетворкинг", 9, "netvorking", 1, null },
                    { 902, "#FD79A8", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Встречи стартаперов", 2, "🚀", true, "Стартапы", 9, "startapy", 1, null },
                    { 903, "#FD79A8", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Конференции, форумы", 3, "📊", true, "Конференции", 9, "konferentsii", 1, null },
                    { 1001, "#00CEC9", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Настольные игры, мафия", 1, "🎲", true, "Настольные игры", 10, "nastolnye-igry", 1, null },
                    { 1002, "#00CEC9", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Компьютерные игры", 2, "🖥️", true, "Киберспорт", 10, "kibersport", 1, null },
                    { 1003, "#00CEC9", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Интеллектуальные квизы", 3, "🧠", true, "Квизы", 10, "kvizy", 1, null },
                    { 1004, "#00CEC9", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Квест-комнаты", 4, "🔍", true, "Квесты", 10, "kvesty", 1, null },
                    { 1101, "#FDCB6E", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Поездки по городам", 1, "🏙️", true, "Городские поездки", 11, "gorodskie-poezdki", 1, null },
                    { 1102, "#FDCB6E", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Автопутешествия, road trips", 2, "🚗", true, "Автопутешествия", 11, "avtoputeshestviya", 1, null },
                    { 1103, "#FDCB6E", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Экотуризм, природа", 3, "🌿", true, "Экотуризм", 11, "ekoturizm", 1, null },
                    { 1201, "#E17055", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Йога, пилатес", 1, "🧘‍♀️", true, "Йога", 12, "ioga", 1, null },
                    { 1202, "#E17055", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Медитация, дыхательные практики", 2, "🕯️", true, "Медитация", 12, "meditatsiya", 1, null },
                    { 1203, "#E17055", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Спа, велнес", 3, "💆", true, "Спа", 12, "spa", 1, null },
                    { 1301, "#6C5CE7", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Приюты, помощь животным", 1, "🐕", true, "Помощь животным", 13, "pomosch-zhivotnym", 1, null },
                    { 1302, "#6C5CE7", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Экологические акции", 2, "♻️", true, "Экология", 13, "ekologiya", 1, null },
                    { 1303, "#6C5CE7", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Благотворительные мероприятия", 3, "❤️", true, "Благотворительность", 13, "blagotvoritelnost", 1, null },
                    { 1401, "#00B894", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Походы в кино", 1, "🍿", true, "Кинотеатры", 14, "kinoteatry", 1, null },
                    { 1402, "#00B894", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Обсуждения фильмов", 2, "🎞️", true, "Киноклубы", 14, "kinokluby", 1, null },
                    { 1501, "#0984E3", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Фотопрогулки, фотосессии", 1, "🌅", true, "Фотопрогулки", 15, "fotoprogulki", 1, null },
                    { 1502, "#0984E3", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Курсы фотографии", 2, "📸", true, "Обучение", 15, "obuchenie", 1, null }
                });

            migrationBuilder.CreateIndex(
                name: "ix_activity_categories_display_order",
                schema: "events",
                table: "activity_categories",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "ix_activity_categories_name",
                schema: "events",
                table: "activity_categories",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_activity_categories_parent_id",
                schema: "events",
                table: "activity_categories",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_activity_categories_slug",
                schema: "events",
                table: "activity_categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_activity_categories_status",
                schema: "events",
                table: "activity_categories",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_activity_categories_status_display_order",
                schema: "events",
                table: "activity_categories",
                columns: new[] { "status", "display_order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_categories",
                schema: "events");

            migrationBuilder.DropTable(
                name: "inbox_message_consumers",
                schema: "events");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "events");

            migrationBuilder.DropTable(
                name: "outbox_message_consumers",
                schema: "events");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "events");
        }
    }
}
