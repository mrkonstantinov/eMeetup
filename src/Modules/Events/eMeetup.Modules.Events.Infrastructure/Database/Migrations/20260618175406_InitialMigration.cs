using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace eMeetup.Modules.Events.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "events");

            migrationBuilder.CreateTable(
                name: "events",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_events", x => x.id);
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

            migrationBuilder.CreateTable(
                name: "participants",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    user_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    gender = table.Column<int>(type: "integer", nullable: false),
                    synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_participants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tag_groups",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, defaultValue: ""),
                    picture_file_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValue: ""),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tag_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "event_sessions",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    starts_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ends_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    locality = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    latitude = table.Column<double>(type: "double precision", precision: 10, scale: 8, nullable: true),
                    longitude = table.Column<double>(type: "double precision", precision: 11, scale: 8, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    canceled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_sessions", x => x.id);
                    table.ForeignKey(
                        name: "fk_event_session_event",
                        column: x => x.event_id,
                        principalSchema: "events",
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    slug = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: ""),
                    usage_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    tag_group_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.id);
                    table.ForeignKey(
                        name: "fk_tags_tag_groups_tag_group_id",
                        column: x => x.tag_group_id,
                        principalSchema: "events",
                        principalTable: "tag_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "mate_types",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    allocated_slots = table.Column<int>(type: "integer", nullable: false),
                    budget = table.Column<decimal>(type: "numeric", nullable: true),
                    min_age = table.Column<int>(type: "integer", nullable: true),
                    max_age = table.Column<int>(type: "integer", nullable: true),
                    gender = table.Column<int>(type: "integer", nullable: true),
                    preferred_gender = table.Column<int>(type: "integer", nullable: true),
                    preferred_age_range = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mate_types", x => x.id);
                    table.ForeignKey(
                        name: "fk_mate_type_event_session",
                        column: x => x.event_session_id,
                        principalSchema: "events",
                        principalTable: "event_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_tags",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_tags", x => x.id);
                    table.ForeignKey(
                        name: "fk_event_tags_events_event_id",
                        column: x => x.event_id,
                        principalSchema: "events",
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_event_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalSchema: "events",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "events",
                table: "tag_groups",
                columns: new[] { "id", "description", "display_order", "is_active", "name", "picture_file_name" },
                values: new object[,]
                {
                    { 1, "Бег, плавание, велоспорт и другие спортивные активности", 1, true, "Активный образ жизни", "1.webp" },
                    { 2, "Лыжи, сноуборд, коньки и тюбинг", 2, true, "Зимние активности", "2.webp" },
                    { 3, "Походы, велотуры и пикники на природе", 3, true, "Приключения и активный отдых", "3.webp" },
                    { 4, "Пакрафтинг, САП, каякинг и другие водные походы", 4, true, "Водные приключения", "4.webp" },
                    { 5, "Музыкальные, гастрономические и культурные фестивали", 5, true, "Фестивали и мероприятия", "5.webp" },
                    { 6, "Городские поездки, экскурсии, гастротуры и автопутешествия", 6, true, "Путешествия", "6.webp" },
                    { 7, "Кино, театр, концерты и рок-шоу", 7, true, "Культура и развлечения", "7.webp" },
                    { 8, "Встречи в пабах, летние веранды и приятное времяпрепровождение", 8, true, "Отдых и тусовки", "8.webp" },
                    { 9, "Марафоны, триатлоны, соревнования и забеги в других городах", 9, true, "Спортивные события", "9.webp" }
                });

            migrationBuilder.InsertData(
                schema: "events",
                table: "tags",
                columns: new[] { "id", "description", "is_active", "name", "slug", "tag_group_id" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-aaaaaaaaaaaa"), "Занятия бегом и пробежки", true, "Бег", "бег", 1 },
                    { new Guid("11111111-1111-1111-1111-bbbbbbbbbbbb"), "Тренировки в бассейне", true, "Плавание", "плавание", 1 },
                    { new Guid("11111111-1111-1111-1111-cccccccccccc"), "Поездки по городу и шоссе", true, "Велоспорт", "велоспорт", 1 },
                    { new Guid("11111111-1111-1111-1111-dddddddddddd"), "Фитнес и силовые тренировки", true, "Тренажерный зал", "тренажерный-зал", 1 },
                    { new Guid("11111111-1111-1111-1111-eeeeeeeeeeee"), "Занятия йогой и растяжка", true, "Йога", "йога", 1 },
                    { new Guid("22222222-2222-2222-2222-aaaaaaaaaaaa"), "Катание на склонах", true, "Горные лыжи", "горные-лыжи", 2 },
                    { new Guid("22222222-2222-2222-2222-bbbbbbbbbbbb"), "Скандинавская ходьба на лыжах по трассам", true, "Беговые лыжи", "беговые-лыжи", 2 },
                    { new Guid("22222222-2222-2222-2222-cccccccccccc"), "Катание на склонах и в парках", true, "Сноуборд", "сноуборд", 2 },
                    { new Guid("22222222-2222-2222-2222-dddddddddddd"), "Катание на ледовых катках", true, "Катание на коньках", "катание-на-коньках", 2 },
                    { new Guid("22222222-2222-2222-2222-eeeeeeeeeeee"), "Катание на ватрушках с горок", true, "Тюбинг", "тюбинг", 2 },
                    { new Guid("22222222-2222-2222-2222-ffffffffffff"), "Походы на снегоступах", true, "Зимний поход", "зимний-поход", 2 },
                    { new Guid("33333333-3333-3333-3333-aaaaaaaaaaaa"), "Дневные походы по тропам", true, "Поход", "поход", 3 },
                    { new Guid("33333333-3333-3333-3333-bbbbbbbbbbbb"), "Многодневные велосипедные путешествия", true, "Велотур", "велотур", 3 },
                    { new Guid("33333333-3333-3333-3333-cccccccccccc"), "Трапезы на природе", true, "Пикник", "пикник", 3 },
                    { new Guid("33333333-3333-3333-3333-dddddddddddd"), "Ночёвки на природе", true, "Кемпинг", "кемпинг", 3 },
                    { new Guid("33333333-3333-3333-3333-eeeeeeeeeeee"), "Бег по природным тропам", true, "Трейлраннинг", "трейлраннинг", 3 },
                    { new Guid("44444444-4444-4444-4444-aaaaaaaaaaaa"), "Сплав на лёгких надувных лодках", true, "Пакрафтинг", "пакрафтинг", 4 },
                    { new Guid("44444444-4444-4444-4444-bbbbbbbbbbbb"), "Катание на доске с веслом стоя", true, "САП-сёрфинг", "сап-сёрфинг", 4 },
                    { new Guid("44444444-4444-4444-4444-cccccccccccc"), "Сплав на каяках по рекам и озёрам", true, "Каякинг", "каякинг", 4 },
                    { new Guid("44444444-4444-4444-4444-dddddddddddd"), "Путешествия на каноэ по спокойной воде", true, "Каноэ", "каноэ", 4 },
                    { new Guid("44444444-4444-4444-4444-eeeeeeeeeeee"), "Сплав по бурной воде", true, "Рафтинг", "рафтинг", 4 },
                    { new Guid("44444444-4444-4444-4444-ffffffffffff"), "Плавание в озёрах и морях", true, "Плавание на открытой воде", "плавание-на-открытой-воде", 4 },
                    { new Guid("55555555-5555-5555-5555-aaaaaaaaaaaa"), "Фестивали живой музыки и концертов", true, "Музыкальный фестиваль", "музыкальный-фестиваль", 5 },
                    { new Guid("55555555-5555-5555-5555-bbbbbbbbbbbb"), "Кулинарные мероприятия", true, "Гастрофестиваль", "гастрофестиваль", 5 },
                    { new Guid("55555555-5555-5555-5555-cccccccccccc"), "Традиционные праздники", true, "Культурный фестиваль", "культурный-фестиваль", 5 },
                    { new Guid("55555555-5555-5555-5555-dddddddddddd"), "Местные ярмарки и гуляния", true, "Городской праздник", "городской-праздник", 5 },
                    { new Guid("55555555-5555-5555-5555-eeeeeeeeeeee"), "Дегустации крафтового пива", true, "Пивной фестиваль", "пивной-фестиваль", 5 },
                    { new Guid("66666666-6666-6666-6666-aaaaaaaaaaaa"), "Осмотр достопримечательностей", true, "Городская поездка", "городская-поездка", 6 },
                    { new Guid("66666666-6666-6666-6666-bbbbbbbbbbbb"), "Кулинарные и винные туры", true, "Гастротур", "гастротур", 6 },
                    { new Guid("66666666-6666-6666-6666-cccccccccccc"), "Путешествия на машине", true, "Автопутешествие", "автопутешествие", 6 },
                    { new Guid("66666666-6666-6666-6666-dddddddddddd"), "Экскурсии по музеям и архитектуре", true, "Культурный тур", "культурный-тур", 6 },
                    { new Guid("66666666-6666-6666-6666-eeeeeeeeeeee"), "Короткие поездки за город", true, "Уикенд за городом", "уикенд-за-городом", 6 },
                    { new Guid("77777777-7777-7777-7777-aaaaaaaaaaaa"), "Фильмы и кинопоказы", true, "Кино", "кино", 7 },
                    { new Guid("77777777-7777-7777-7777-bbbbbbbbbbbb"), "Спектакли и театральные постановки", true, "Театр", "театр", 7 },
                    { new Guid("77777777-7777-7777-7777-cccccccccccc"), "Концерты живой музыки", true, "Концерт", "концерт", 7 },
                    { new Guid("77777777-7777-7777-7777-dddddddddddd"), "Рок и метал концерты", true, "Рок-концерт", "рок-концерт", 7 },
                    { new Guid("77777777-7777-7777-7777-eeeeeeeeeeee"), "Художественные галереи и выставки", true, "Выставка искусств", "выставка-искусств", 7 },
                    { new Guid("77777777-7777-7777-7777-ffffffffffff"), "Юмористические выступления", true, "Стендап", "стендап", 7 },
                    { new Guid("88888888-8888-8888-8888-aaaaaaaaaaaa"), "Встречи в барах и пабах", true, "Паб", "паб", 8 },
                    { new Guid("88888888-8888-8888-8888-bbbbbbbbbbbb"), "Летние кафе и террасы", true, "Веранда", "веранда", 8 },
                    { new Guid("88888888-8888-8888-8888-cccccccccccc"), "Неформальные встречи за кофе", true, "Кофе-дейт", "кофе-дейт", 8 },
                    { new Guid("88888888-8888-8888-8888-dddddddddddd"), "Вечера настольных игр", true, "Настольные игры", "настольные-игры", 8 },
                    { new Guid("88888888-8888-8888-8888-eeeeeeeeeeee"), "Неспешные встречи в парке", true, "Пикник в парке", "пикник-в-парке", 8 },
                    { new Guid("88888888-8888-8888-8888-ffffffffffff"), "Расслабленный летний отдых в гамаке на природе", true, "Отдых в гамаке", "отдых-в-гамаке", 8 },
                    { new Guid("99999999-9999-9999-9999-aaaaaaaaaaaa"), "Соревнования по плаванию, велоспорту и бегу", true, "Триатлон", "триатлон", 9 },
                    { new Guid("99999999-9999-9999-9999-bbbbbbbbbbbb"), "Участие в марафонах в других городах", true, "Марафон", "марафон", 9 },
                    { new Guid("99999999-9999-9999-9999-cccccccccccc"), "Забеги с препятствиями", true, "Гонка с препятствиями", "гонка-с-препятствиями", 9 },
                    { new Guid("99999999-9999-9999-9999-dddddddddddd"), "Соревновательные велозаезды", true, "Велосипедная гонка", "велосипедная-гонка", 9 },
                    { new Guid("99999999-9999-9999-9999-eeeeeeeeeeee"), "Заплывы на открытой воде или в бассейне", true, "Соревнования по плаванию", "соревнования-по-плаванию", 9 },
                    { new Guid("99999999-9999-9999-9999-ffffffffffff"), "Соревнования по бегу по бездорожью", true, "Трейловый забег", "трейловый-забег", 9 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_event_sessions_event_id",
                schema: "events",
                table: "event_sessions",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_sessions_event_status",
                schema: "events",
                table: "event_sessions",
                columns: new[] { "event_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_event_sessions_starts_at_utc",
                schema: "events",
                table: "event_sessions",
                column: "starts_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_event_sessions_status_start_date",
                schema: "events",
                table: "event_sessions",
                columns: new[] { "status", "starts_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_event_tags_event_id",
                schema: "events",
                table: "event_tags",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_tags_tag_id",
                schema: "events",
                table: "event_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_mate_types_event_session_id",
                schema: "events",
                table: "mate_types",
                column: "event_session_id");

            migrationBuilder.CreateIndex(
                name: "ix_mate_types_gender",
                schema: "events",
                table: "mate_types",
                column: "gender");

            migrationBuilder.CreateIndex(
                name: "ix_mate_types_session_priority",
                schema: "events",
                table: "mate_types",
                columns: new[] { "event_session_id", "priority" });

            migrationBuilder.CreateIndex(
                name: "ix_tag_groups_display_order",
                schema: "events",
                table: "tag_groups",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "ix_tag_groups_is_active",
                schema: "events",
                table: "tag_groups",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_tag_groups_name",
                schema: "events",
                table: "tag_groups",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tags_is_active",
                schema: "events",
                table: "tags",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_tags_name",
                schema: "events",
                table: "tags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tags_slug",
                schema: "events",
                table: "tags",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tags_tag_group_id",
                schema: "events",
                table: "tags",
                column: "tag_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_tags_usage_count",
                schema: "events",
                table: "tags",
                column: "usage_count");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_tags",
                schema: "events");

            migrationBuilder.DropTable(
                name: "inbox_message_consumers",
                schema: "events");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "events");

            migrationBuilder.DropTable(
                name: "mate_types",
                schema: "events");

            migrationBuilder.DropTable(
                name: "outbox_message_consumers",
                schema: "events");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "events");

            migrationBuilder.DropTable(
                name: "participants",
                schema: "events");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "events");

            migrationBuilder.DropTable(
                name: "event_sessions",
                schema: "events");

            migrationBuilder.DropTable(
                name: "tag_groups",
                schema: "events");

            migrationBuilder.DropTable(
                name: "events",
                schema: "events");
        }
    }
}
