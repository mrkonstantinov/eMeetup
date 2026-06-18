using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace eMeetup.Modules.Users.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "users");

            migrationBuilder.CreateTable(
                name: "inbox_message_consumers",
                schema: "users",
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
                schema: "users",
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
                schema: "users",
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
                schema: "users",
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
                name: "permissions",
                schema: "users",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissions", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "users",
                columns: table => new
                {
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "tag_groups",
                schema: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, defaultValue: ""),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValue: ""),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tag_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identity_id = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    user_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    gender = table.Column<int>(type: "integer", nullable: false),
                    locality = table.Column<string>(type: "text", nullable: true),
                    street = table.Column<string>(type: "text", nullable: true),
                    bio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    profile_image_url = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_active = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "users",
                columns: table => new
                {
                    permission_code = table.Column<string>(type: "character varying(100)", nullable: false),
                    role_name = table.Column<string>(type: "character varying(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => new { x.permission_code, x.role_name });
                    table.ForeignKey(
                        name: "fk_role_permissions_permissions_permission_code",
                        column: x => x.permission_code,
                        principalSchema: "users",
                        principalTable: "permissions",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_permissions_roles_role_name",
                        column: x => x.role_name,
                        principalSchema: "users",
                        principalTable: "roles",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    slug = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: ""),
                    usage_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    tag_group_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.id);
                    table.ForeignKey(
                        name: "fk_tags_tag_group_tag_group_id",
                        column: x => x.tag_group_id,
                        principalSchema: "users",
                        principalTable: "tag_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "user_photos",
                schema: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_photos", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_photos_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "users",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "users",
                columns: table => new
                {
                    role_name = table.Column<string>(type: "character varying(50)", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => new { x.role_name, x.user_id });
                    table.ForeignKey(
                        name: "fk_user_roles_roles_roles_name",
                        column: x => x.role_name,
                        principalSchema: "users",
                        principalTable: "roles",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "users",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_interests",
                schema: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_interests", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_interests_tags_tag_id",
                        column: x => x.tag_id,
                        principalSchema: "users",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_interests_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "users",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Junction table for user interests and tags");

            migrationBuilder.InsertData(
                schema: "users",
                table: "permissions",
                column: "code",
                values: new object[]
                {
                    "event-statistics:read",
                    "events:read",
                    "events:search",
                    "events:update",
                    "tags:read",
                    "users:read",
                    "users:update"
                });

            migrationBuilder.InsertData(
                schema: "users",
                table: "roles",
                column: "name",
                values: new object[]
                {
                    "Administrator",
                    "Member"
                });

            migrationBuilder.InsertData(
                schema: "users",
                table: "tag_groups",
                columns: new[] { "id", "description", "display_order", "icon", "is_active", "name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Бег, плавание, велоспорт и другие спортивные активности", 1, "🏃", true, "Активный образ жизни" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Лыжи, сноуборд, коньки и тюбинг", 2, "❄️", true, "Зимние активности" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Походы, велотуры и пикники на природе", 3, "⛰️", true, "Приключения и активный отдых" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Пакрафтинг, САП, каякинг и другие водные походы", 4, "🚣", true, "Водные приключения" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "Музыкальные, гастрономические и культурные фестивали", 5, "🎪", true, "Фестивали и мероприятия" },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "Городские поездки, экскурсии, гастротуры и автопутешествия", 6, "✈️", true, "Путешествия" },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "Кино, театр, концерты и рок-шоу", 7, "🎭", true, "Культура и развлечения" },
                    { new Guid("88888888-8888-8888-8888-888888888888"), "Встречи в пабах, летние веранды и приятное времяпрепровождение", 8, "😎", true, "Отдых и тусовки" },
                    { new Guid("99999999-9999-9999-9999-999999999999"), "Марафоны, триатлоны, соревнования и забеги в других городах", 9, "🏆", true, "Спортивные события" }
                });

            migrationBuilder.InsertData(
                schema: "users",
                table: "role_permissions",
                columns: new[] { "permission_code", "role_name" },
                values: new object[,]
                {
                    { "event-statistics:read", "Administrator" },
                    { "events:read", "Administrator" },
                    { "events:read", "Member" },
                    { "events:search", "Administrator" },
                    { "events:search", "Member" },
                    { "events:update", "Administrator" },
                    { "events:update", "Member" },
                    { "tags:read", "Administrator" },
                    { "tags:read", "Member" },
                    { "users:read", "Administrator" },
                    { "users:read", "Member" },
                    { "users:update", "Administrator" },
                    { "users:update", "Member" }
                });

            migrationBuilder.InsertData(
                schema: "users",
                table: "tags",
                columns: new[] { "id", "description", "is_active", "name", "slug", "tag_group_id" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-aaaaaaaaaaaa"), "Занятия бегом и пробежки", true, "Бег", "бег", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-1111-1111-1111-bbbbbbbbbbbb"), "Тренировки в бассейне", true, "Плавание", "плавание", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-1111-1111-1111-cccccccccccc"), "Поездки по городу и шоссе", true, "Велоспорт", "велоспорт", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-1111-1111-1111-dddddddddddd"), "Фитнес и силовые тренировки", true, "Тренажерный зал", "тренажерный-зал", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-1111-1111-1111-eeeeeeeeeeee"), "Занятия йогой и растяжка", true, "Йога", "йога", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("22222222-2222-2222-2222-aaaaaaaaaaaa"), "Катание на склонах", true, "Горные лыжи", "горные-лыжи", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("22222222-2222-2222-2222-bbbbbbbbbbbb"), "Скандинавская ходьба на лыжах по трассам", true, "Беговые лыжи", "беговые-лыжи", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("22222222-2222-2222-2222-cccccccccccc"), "Катание на склонах и в парках", true, "Сноуборд", "сноуборд", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("22222222-2222-2222-2222-dddddddddddd"), "Катание на ледовых катках", true, "Катание на коньках", "катание-на-коньках", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("22222222-2222-2222-2222-eeeeeeeeeeee"), "Катание на ватрушках с горок", true, "Тюбинг", "тюбинг", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("22222222-2222-2222-2222-ffffffffffff"), "Походы на снегоступах", true, "Зимний поход", "зимний-поход", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("33333333-3333-3333-3333-aaaaaaaaaaaa"), "Дневные походы по тропам", true, "Поход", "поход", new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("33333333-3333-3333-3333-bbbbbbbbbbbb"), "Многодневные велосипедные путешествия", true, "Велотур", "велотур", new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("33333333-3333-3333-3333-cccccccccccc"), "Трапезы на природе", true, "Пикник", "пикник", new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("33333333-3333-3333-3333-dddddddddddd"), "Ночёвки на природе", true, "Кемпинг", "кемпинг", new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("33333333-3333-3333-3333-eeeeeeeeeeee"), "Бег по природным тропам", true, "Трейлраннинг", "трейлраннинг", new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("44444444-4444-4444-4444-aaaaaaaaaaaa"), "Сплав на лёгких надувных лодках", true, "Пакрафтинг", "пакрафтинг", new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("44444444-4444-4444-4444-bbbbbbbbbbbb"), "Катание на доске с веслом стоя", true, "САП-сёрфинг", "сап-сёрфинг", new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("44444444-4444-4444-4444-cccccccccccc"), "Сплав на каяках по рекам и озёрам", true, "Каякинг", "каякинг", new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("44444444-4444-4444-4444-dddddddddddd"), "Путешествия на каноэ по спокойной воде", true, "Каноэ", "каноэ", new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("44444444-4444-4444-4444-eeeeeeeeeeee"), "Сплав по бурной воде", true, "Рафтинг", "рафтинг", new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("44444444-4444-4444-4444-ffffffffffff"), "Плавание в озёрах и морях", true, "Плавание на открытой воде", "плавание-на-открытой-воде", new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("55555555-5555-5555-5555-aaaaaaaaaaaa"), "Фестивали живой музыки и концертов", true, "Музыкальный фестиваль", "музыкальный-фестиваль", new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("55555555-5555-5555-5555-bbbbbbbbbbbb"), "Кулинарные мероприятия", true, "Гастрофестиваль", "гастрофестиваль", new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("55555555-5555-5555-5555-cccccccccccc"), "Традиционные праздники", true, "Культурный фестиваль", "культурный-фестиваль", new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("55555555-5555-5555-5555-dddddddddddd"), "Местные ярмарки и гуляния", true, "Городской праздник", "городской-праздник", new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("55555555-5555-5555-5555-eeeeeeeeeeee"), "Дегустации крафтового пива", true, "Пивной фестиваль", "пивной-фестиваль", new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("66666666-6666-6666-6666-aaaaaaaaaaaa"), "Осмотр достопримечательностей", true, "Городская поездка", "городская-поездка", new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("66666666-6666-6666-6666-bbbbbbbbbbbb"), "Кулинарные и винные туры", true, "Гастротур", "гастротур", new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("66666666-6666-6666-6666-cccccccccccc"), "Путешествия на машине", true, "Автопутешествие", "автопутешествие", new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("66666666-6666-6666-6666-dddddddddddd"), "Экскурсии по музеям и архитектуре", true, "Культурный тур", "культурный-тур", new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("66666666-6666-6666-6666-eeeeeeeeeeee"), "Короткие поездки за город", true, "Уикенд за городом", "уикенд-за-городом", new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("77777777-7777-7777-7777-aaaaaaaaaaaa"), "Фильмы и кинопоказы", true, "Кино", "кино", new Guid("77777777-7777-7777-7777-777777777777") },
                    { new Guid("77777777-7777-7777-7777-bbbbbbbbbbbb"), "Спектакли и театральные постановки", true, "Театр", "театр", new Guid("77777777-7777-7777-7777-777777777777") },
                    { new Guid("77777777-7777-7777-7777-cccccccccccc"), "Концерты живой музыки", true, "Концерт", "концерт", new Guid("77777777-7777-7777-7777-777777777777") },
                    { new Guid("77777777-7777-7777-7777-dddddddddddd"), "Рок и метал концерты", true, "Рок-концерт", "рок-концерт", new Guid("77777777-7777-7777-7777-777777777777") },
                    { new Guid("77777777-7777-7777-7777-eeeeeeeeeeee"), "Художественные галереи и выставки", true, "Выставка искусств", "выставка-искусств", new Guid("77777777-7777-7777-7777-777777777777") },
                    { new Guid("77777777-7777-7777-7777-ffffffffffff"), "Юмористические выступления", true, "Стендап", "стендап", new Guid("77777777-7777-7777-7777-777777777777") },
                    { new Guid("88888888-8888-8888-8888-aaaaaaaaaaaa"), "Встречи в барах и пабах", true, "Паб", "паб", new Guid("88888888-8888-8888-8888-888888888888") },
                    { new Guid("88888888-8888-8888-8888-bbbbbbbbbbbb"), "Летние кафе и террасы", true, "Веранда", "веранда", new Guid("88888888-8888-8888-8888-888888888888") },
                    { new Guid("88888888-8888-8888-8888-cccccccccccc"), "Неформальные встречи за кофе", true, "Кофе-дейт", "кофе-дейт", new Guid("88888888-8888-8888-8888-888888888888") },
                    { new Guid("88888888-8888-8888-8888-dddddddddddd"), "Вечера настольных игр", true, "Настольные игры", "настольные-игры", new Guid("88888888-8888-8888-8888-888888888888") },
                    { new Guid("88888888-8888-8888-8888-eeeeeeeeeeee"), "Неспешные встречи в парке", true, "Пикник в парке", "пикник-в-парке", new Guid("88888888-8888-8888-8888-888888888888") },
                    { new Guid("88888888-8888-8888-8888-ffffffffffff"), "Расслабленный летний отдых в гамаке на природе", true, "Отдых в гамаке", "отдых-в-гамаке", new Guid("88888888-8888-8888-8888-888888888888") },
                    { new Guid("99999999-9999-9999-9999-aaaaaaaaaaaa"), "Соревнования по плаванию, велоспорту и бегу", true, "Триатлон", "триатлон", new Guid("99999999-9999-9999-9999-999999999999") },
                    { new Guid("99999999-9999-9999-9999-bbbbbbbbbbbb"), "Участие в марафонах в других городах", true, "Марафон", "марафон", new Guid("99999999-9999-9999-9999-999999999999") },
                    { new Guid("99999999-9999-9999-9999-cccccccccccc"), "Забеги с препятствиями", true, "Гонка с препятствиями", "гонка-с-препятствиями", new Guid("99999999-9999-9999-9999-999999999999") },
                    { new Guid("99999999-9999-9999-9999-dddddddddddd"), "Соревновательные велозаезды", true, "Велосипедная гонка", "велосипедная-гонка", new Guid("99999999-9999-9999-9999-999999999999") },
                    { new Guid("99999999-9999-9999-9999-eeeeeeeeeeee"), "Заплывы на открытой воде или в бассейне", true, "Соревнования по плаванию", "соревнования-по-плаванию", new Guid("99999999-9999-9999-9999-999999999999") },
                    { new Guid("99999999-9999-9999-9999-ffffffffffff"), "Соревнования по бегу по бездорожью", true, "Трейловый забег", "трейловый-забег", new Guid("99999999-9999-9999-9999-999999999999") }
                });

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_role_name",
                schema: "users",
                table: "role_permissions",
                column: "role_name");

            migrationBuilder.CreateIndex(
                name: "ix_tag_groups_display_order",
                schema: "users",
                table: "tag_groups",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "ix_tag_groups_is_active",
                schema: "users",
                table: "tag_groups",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_tag_groups_name",
                schema: "users",
                table: "tag_groups",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tags_is_active",
                schema: "users",
                table: "tags",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_tags_name",
                schema: "users",
                table: "tags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tags_slug",
                schema: "users",
                table: "tags",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tags_tag_group_id",
                schema: "users",
                table: "tags",
                column: "tag_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_tags_usage_count",
                schema: "users",
                table: "tags",
                column: "usage_count");

            migrationBuilder.CreateIndex(
                name: "ix_user_interests_created_at",
                schema: "users",
                table: "user_interests",
                column: "created_at")
                .Annotation("Npgsql:IndexMethod", "brin");

            migrationBuilder.CreateIndex(
                name: "ix_user_interests_tag_id",
                schema: "users",
                table: "user_interests",
                column: "tag_id")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "ix_user_interests_user_id",
                schema: "users",
                table: "user_interests",
                column: "user_id")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "ix_user_interests_user_tag_unique",
                schema: "users",
                table: "user_interests",
                columns: new[] { "user_id", "tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_photos_user_id",
                schema: "users",
                table: "user_photos",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_photos_user_id_display_order",
                schema: "users",
                table: "user_photos",
                columns: new[] { "user_id", "display_order" });

            migrationBuilder.CreateIndex(
                name: "ix_user_photos_user_id_is_primary",
                schema: "users",
                table: "user_photos",
                columns: new[] { "user_id", "is_primary" },
                filter: "is_primary = true");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_user_id",
                schema: "users",
                table: "user_roles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "users",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_identity_id",
                schema: "users",
                table: "users",
                column: "identity_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_message_consumers",
                schema: "users");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "users");

            migrationBuilder.DropTable(
                name: "outbox_message_consumers",
                schema: "users");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "users");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "users");

            migrationBuilder.DropTable(
                name: "user_interests",
                schema: "users");

            migrationBuilder.DropTable(
                name: "user_photos",
                schema: "users");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "users");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "users");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "users");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "users");

            migrationBuilder.DropTable(
                name: "users",
                schema: "users");

            migrationBuilder.DropTable(
                name: "tag_groups",
                schema: "users");
        }
    }
}
