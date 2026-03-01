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
                    bio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    profile_picture_url = table.Column<string>(type: "text", nullable: true),
                    location_latitude = table.Column<double>(type: "numeric(9,6)", nullable: true),
                    location_longitude = table.Column<double>(type: "numeric(9,6)", nullable: true),
                    location_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    location_street = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Running, swimming, cycling and other sports activities", 1, "🏃", true, "Active Lifestyle" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Skiing, snowboarding, ice skating and snow tubing", 2, "❄️", true, "Winter Activities" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Hiking, biking tours and picnics in nature", 3, "⛰️", true, "Adventure & Outdoors" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Packrafting, SUP, kayaking and other water hikes", 4, "🚣", true, "Water Adventures" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "Music, food and cultural festivals celebrations", 5, "🎪", true, "Festivals & Events" },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "City trips, sightseeing, food tours and road trips", 6, "✈️", true, "Travel & Exploration" },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "Cinema, theater, concerts and rock shows", 7, "🎭", true, "Culture & Entertainment" },
                    { new Guid("88888888-8888-8888-8888-888888888888"), "Pub meetups, terrace cafes and relaxing time", 8, "😎", true, "Chill & Hangout" },
                    { new Guid("99999999-9999-9999-9999-999999999999"), "Marathons, triathlons, competitions and races in other cities", 9, "🏆", true, "Sports Events" }
                });

            migrationBuilder.InsertData(
                schema: "users",
                table: "role_permissions",
                columns: new[] { "permission_code", "role_name" },
                values: new object[,]
                {
                    { "event-statistics:read", "Administrator" },
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
                    { new Guid("11111111-1111-1111-1111-aaaaaaaaaaaa"), "Jogging and running activities", true, "Running", "running", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-1111-1111-1111-bbbbbbbbbbbb"), "Pool swimming and training", true, "Swimming", "swimming", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-1111-1111-1111-cccccccccccc"), "Road and city bike rides", true, "Cycling", "cycling", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-1111-1111-1111-dddddddddddd"), "Fitness and strength training", true, "Gym Workout", "gym-workout", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("11111111-1111-1111-1111-eeeeeeeeeeee"), "Yoga and stretching sessions", true, "Yoga", "yoga", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("22222222-2222-2222-2222-aaaaaaaaaaaa"), "Downhill skiing on slopes", true, "Alpine Skiing", "alpine-skiing", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("22222222-2222-2222-2222-bbbbbbbbbbbb"), "Nordic skiing on trails", true, "Cross-Country Skiing", "cross-country-skiing", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("22222222-2222-2222-2222-cccccccccccc"), "Snowboarding on slopes and parks", true, "Snowboarding", "snowboarding", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("22222222-2222-2222-2222-dddddddddddd"), "Skating on ice rinks", true, "Ice Skating", "ice-skating", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("22222222-2222-2222-2222-eeeeeeeeeeee"), "Snow sliding with tubes", true, "Snow Tubing", "snow-tubing", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("22222222-2222-2222-2222-ffffffffffff"), "Hiking with snowshoes", true, "Winter Hiking", "winter-hiking", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("33333333-3333-3333-3333-aaaaaaaaaaaa"), "Day hiking on trails", true, "Hike", "hike", new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("33333333-3333-3333-3333-bbbbbbbbbbbb"), "Multi-day cycling adventures", true, "Bike Touring", "bike-touring", new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("33333333-3333-3333-3333-cccccccccccc"), "Outdoor meals in nature", true, "Picnic", "picnic", new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("33333333-3333-3333-3333-dddddddddddd"), "Overnight stays in nature", true, "Camping", "camping", new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("33333333-3333-3333-3333-eeeeeeeeeeee"), "Running on nature trails", true, "Trail Running", "trail-running", new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("44444444-4444-4444-4444-aaaaaaaaaaaa"), "Lightweight portable rafting", true, "Packrafting", "packrafting", new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("44444444-4444-4444-4444-bbbbbbbbbbbb"), "Stand-Up Paddleboarding", true, "SUP", "sup", new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("44444444-4444-4444-4444-cccccccccccc"), "Kayaking on rivers and lakes", true, "Kayaking", "kayaking", new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("44444444-4444-4444-4444-dddddddddddd"), "Canoe trips on calm waters", true, "Canoeing", "canoeing", new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("44444444-4444-4444-4444-eeeeeeeeeeee"), "White water rafting", true, "Rafting", "rafting", new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("44444444-4444-4444-4444-ffffffffffff"), "Swimming in lakes and seas", true, "Open Water Swimming", "open-water-swimming", new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("55555555-5555-5555-5555-aaaaaaaaaaaa"), "Live music and concerts festivals", true, "Music Festival", "music-festival", new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("55555555-5555-5555-5555-bbbbbbbbbbbb"), "Culinary and gastronomy events", true, "Food Festival", "food-festival", new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("55555555-5555-5555-5555-cccccccccccc"), "Traditional and cultural celebrations", true, "Cultural Festival", "cultural-festival", new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("55555555-5555-5555-5555-dddddddddddd"), "Local city festivals and fairs", true, "City Celebration", "city-celebration", new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("55555555-5555-5555-5555-eeeeeeeeeeee"), "Craft beer and brewery events", true, "Beer Festival", "beer-festival", new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("66666666-6666-6666-6666-aaaaaaaaaaaa"), "Sightseeing in cities", true, "City Trip", "city-trip", new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("66666666-6666-6666-6666-bbbbbbbbbbbb"), "Gastronomy and local food tours", true, "Food Tourism", "food-tourism", new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("66666666-6666-6666-6666-cccccccccccc"), "Traveling by car", true, "Road Trip", "road-trip", new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("66666666-6666-6666-6666-dddddddddddd"), "Museums and architecture tours", true, "Cultural Tour", "cultural-tour", new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("66666666-6666-6666-6666-eeeeeeeeeeee"), "Short trips out of town", true, "Weekend Getaway", "weekend-getaway", new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("77777777-7777-7777-7777-aaaaaaaaaaaa"), "Movies and film screenings", true, "Cinema", "cinema", new Guid("77777777-7777-7777-7777-777777777777") },
                    { new Guid("77777777-7777-7777-7777-bbbbbbbbbbbb"), "Plays and theatrical performances", true, "Theater", "theater", new Guid("77777777-7777-7777-7777-777777777777") },
                    { new Guid("77777777-7777-7777-7777-cccccccccccc"), "Live music performances", true, "Concert", "concert", new Guid("77777777-7777-7777-7777-777777777777") },
                    { new Guid("77777777-7777-7777-7777-dddddddddddd"), "Rock and metal gigs", true, "Rock Show", "rock-show", new Guid("77777777-7777-7777-7777-777777777777") },
                    { new Guid("77777777-7777-7777-7777-eeeeeeeeeeee"), "Gallery and art shows", true, "Art Exhibition", "art-exhibition", new Guid("77777777-7777-7777-7777-777777777777") },
                    { new Guid("77777777-7777-7777-7777-ffffffffffff"), "Stand-up comedy performances", true, "Comedy Show", "comedy-show", new Guid("77777777-7777-7777-7777-777777777777") },
                    { new Guid("88888888-8888-8888-8888-aaaaaaaaaaaa"), "Bar and pub meetups", true, "Pub", "pub", new Guid("88888888-8888-8888-8888-888888888888") },
                    { new Guid("88888888-8888-8888-8888-bbbbbbbbbbbb"), "Outdoor cafes and terraces", true, "Patio", "patio", new Guid("88888888-8888-8888-8888-888888888888") },
                    { new Guid("88888888-8888-8888-8888-cccccccccccc"), "Casual coffee meetings", true, "Coffee Date", "coffee-date", new Guid("88888888-8888-8888-8888-888888888888") },
                    { new Guid("88888888-8888-8888-8888-dddddddddddd"), "Board game nights", true, "Board Games", "board-games", new Guid("88888888-8888-8888-8888-888888888888") },
                    { new Guid("88888888-8888-8888-8888-eeeeeeeeeeee"), "Relaxed park gatherings", true, "Picnic in Park", "picnic-in-park", new Guid("88888888-8888-8888-8888-888888888888") },
                    { new Guid("99999999-9999-9999-9999-aaaaaaaaaaaa"), "Swim, bike, run races", true, "Triathlon", "triathlon", new Guid("99999999-9999-9999-9999-999999999999") },
                    { new Guid("99999999-9999-9999-9999-bbbbbbbbbbbb"), "Running marathons in other cities", true, "Marathon", "marathon", new Guid("99999999-9999-9999-9999-999999999999") },
                    { new Guid("99999999-9999-9999-9999-cccccccccccc"), "Tough mudder and obstacle courses", true, "Obstacle Race", "obstacle-race", new Guid("99999999-9999-9999-9999-999999999999") },
                    { new Guid("99999999-9999-9999-9999-dddddddddddd"), "Competitive bike racing", true, "Cycling Race", "cycling-race", new Guid("99999999-9999-9999-9999-999999999999") },
                    { new Guid("99999999-9999-9999-9999-eeeeeeeeeeee"), "Open water or pool races", true, "Swimming Competition", "swimming-competition", new Guid("99999999-9999-9999-9999-999999999999") },
                    { new Guid("99999999-9999-9999-9999-ffffffffffff"), "Off-road running competitions", true, "Trail Running Race", "trail-running-race", new Guid("99999999-9999-9999-9999-999999999999") }
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
