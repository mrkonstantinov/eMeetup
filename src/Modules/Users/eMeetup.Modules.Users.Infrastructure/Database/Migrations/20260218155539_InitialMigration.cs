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
                    location_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Activities related to water-based recreation", 1, "🌊", true, "Water Sports" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Trail walking, backpacking, and mountain hiking", 2, "🥾", true, "Hiking & Trekking" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Overnight outdoor stays and wilderness camping", 3, "⛺", true, "Camping" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Rock climbing, bouldering, and mountaineering", 4, "🧗", true, "Climbing" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "Snow and ice activities", 5, "⛷️", true, "Winter Sports" },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "Biking, mountain biking, and cycling tours", 6, "🚴", true, "Cycling" },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "Bird watching, nature photography, and wildlife observation", 7, "🦌", true, "Wildlife & Nature" },
                    { new Guid("88888888-8888-8888-8888-888888888888"), "Angling, fly fishing, and ice fishing", 8, "🎣", true, "Fishing" }
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
                    { new Guid("aaaaaaaa-1111-1111-1111-111111111111"), "Skiing on water surface", true, "Waterskiing", "waterskiing", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("aaaaaaaa-1111-1111-1111-222222222222"), "Board riding on wake", true, "Wakeboarding", "wakeboarding", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("aaaaaaaa-1111-1111-1111-333333333333"), "Wind-powered boating", true, "Sailing", "sailing", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("aaaaaaaa-1111-1111-1111-444444444444"), "Paddle in open canoes", true, "Canoeing", "canoeing", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa"), "Paddle through waters in a kayak", true, "Kayaking", "kayaking", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("aaaaaaaa-1111-1111-1111-bbbbbbbbbbbb"), "White water rafting adventures", true, "Rafting", "rafting", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("aaaaaaaa-1111-1111-1111-cccccccccccc"), "Ride the waves", true, "Surfing", "surfing", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("aaaaaaaa-1111-1111-1111-dddddddddddd"), "Explore underwater worlds", true, "Scuba Diving", "scuba-diving", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("aaaaaaaa-1111-1111-1111-eeeeeeeeeeee"), "Surface underwater exploration", true, "Snorkeling", "snorkeling", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("aaaaaaaa-1111-1111-1111-ffffffffffff"), "Paddleboarding on calm waters", true, "Stand Up Paddleboarding", "stand-up-paddleboarding", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("aaaaaaaa-7777-7777-7777-111111111111"), "Searching for wild food", true, "Foraging", "foraging", new Guid("77777777-7777-7777-7777-777777777777") },
                    { new Guid("aaaaaaaa-7777-7777-7777-aaaaaaaaaaaa"), "Observing birds in their habitat", true, "Bird Watching", "bird-watching", new Guid("77777777-7777-7777-7777-777777777777") },
                    { new Guid("aaaaaaaa-7777-7777-7777-bbbbbbbbbbbb"), "Photographing animals in nature", true, "Wildlife Photography", "wildlife-photography", new Guid("77777777-7777-7777-7777-777777777777") },
                    { new Guid("aaaaaaaa-7777-7777-7777-cccccccccccc"), "Capturing landscapes and natural scenes", true, "Nature Photography", "nature-photography", new Guid("77777777-7777-7777-7777-777777777777") },
                    { new Guid("aaaaaaaa-7777-7777-7777-dddddddddddd"), "Observing celestial objects", true, "Stargazing", "stargazing", new Guid("77777777-7777-7777-7777-777777777777") },
                    { new Guid("aaaaaaaa-7777-7777-7777-eeeeeeeeeeee"), "Studying plants and wildflowers", true, "Botany", "botany", new Guid("77777777-7777-7777-7777-777777777777") },
                    { new Guid("aaaaaaaa-7777-7777-7777-ffffffffffff"), "Outdoor treasure hunting with GPS", true, "Geocaching", "geocaching", new Guid("77777777-7777-7777-7777-777777777777") },
                    { new Guid("bbbbbbbb-2222-2222-2222-111111111111"), "Technical hiking with hands-on climbing", true, "Scrambling", "scrambling", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("bbbbbbbb-2222-2222-2222-aaaaaaaaaaaa"), "Single day trail walks", true, "Day Hiking", "day-hiking", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("bbbbbbbb-2222-2222-2222-bbbbbbbbbbbb"), "Multi-day hiking with gear", true, "Backpacking", "backpacking", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("bbbbbbbb-2222-2222-2222-cccccccccccc"), "Running on nature trails", true, "Trail Running", "trail-running", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("bbbbbbbb-2222-2222-2222-dddddddddddd"), "Summiting mountain peaks", true, "Peak Bagging", "peak-bagging", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("bbbbbbbb-2222-2222-2222-eeeeeeeeeeee"), "Leisurely nature exploration", true, "Nature Walks", "nature-walks", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("bbbbbbbb-2222-2222-2222-ffffffffffff"), "Walking with poles", true, "Nordic Walking", "nordic-walking", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("bbbbbbbb-8888-8888-8888-111111111111"), "Fishing from shoreline", true, "Surf Fishing", "surf-fishing", new Guid("88888888-8888-8888-8888-888888888888") },
                    { new Guid("bbbbbbbb-8888-8888-8888-aaaaaaaaaaaa"), "Fishing with artificial flies", true, "Fly Fishing", "fly-fishing", new Guid("88888888-8888-8888-8888-888888888888") },
                    { new Guid("bbbbbbbb-8888-8888-8888-bbbbbbbbbbbb"), "Fishing in ocean waters", true, "Deep Sea Fishing", "deep-sea-fishing", new Guid("88888888-8888-8888-8888-888888888888") },
                    { new Guid("bbbbbbbb-8888-8888-8888-cccccccccccc"), "Fishing through holes in ice", true, "Ice Fishing", "ice-fishing", new Guid("88888888-8888-8888-8888-888888888888") },
                    { new Guid("bbbbbbbb-8888-8888-8888-dddddddddddd"), "Fishing in lakes and rivers", true, "Freshwater Fishing", "freshwater-fishing", new Guid("88888888-8888-8888-8888-888888888888") },
                    { new Guid("bbbbbbbb-8888-8888-8888-eeeeeeeeeeee"), "Fishing from a kayak", true, "Kayak Fishing", "kayak-fishing", new Guid("88888888-8888-8888-8888-888888888888") },
                    { new Guid("bbbbbbbb-8888-8888-8888-ffffffffffff"), "Fishing with a spear", true, "Spearfishing", "spearfishing", new Guid("88888888-8888-8888-8888-888888888888") },
                    { new Guid("cccccccc-3333-3333-3333-111111111111"), "Camping on or near beaches", true, "Beach Camping", "beach-camping", new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("cccccccc-3333-3333-3333-aaaaaaaaaaaa"), "Traditional tent camping", true, "Tent Camping", "tent-camping", new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("cccccccc-3333-3333-3333-bbbbbbbbbbbb"), "Camping with recreational vehicles", true, "RV Camping", "rv-camping", new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("cccccccc-3333-3333-3333-cccccccccccc"), "Remote wilderness camping", true, "Backcountry Camping", "backcountry-camping", new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("cccccccc-3333-3333-3333-dddddddddddd"), "Luxury camping experience", true, "Glamping", "glamping", new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("cccccccc-3333-3333-3333-eeeeeeeeeeee"), "Sleeping in hammocks", true, "Hammock Camping", "hammock-camping", new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("cccccccc-3333-3333-3333-ffffffffffff"), "Cold weather camping", true, "Winter Camping", "winter-camping", new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("dddddddd-4444-4444-4444-111111111111"), "Protected climbing routes with cables", true, "Via Ferrata", "via-ferrata", new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("dddddddd-4444-4444-4444-aaaaaaaaaaaa"), "Climbing natural rock formations", true, "Rock Climbing", "rock-climbing", new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("dddddddd-4444-4444-4444-bbbbbbbbbbbb"), "Low-level climbing without ropes", true, "Bouldering", "bouldering", new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("dddddddd-4444-4444-4444-cccccccccccc"), "Climbing with fixed anchors", true, "Sport Climbing", "sport-climbing", new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("dddddddd-4444-4444-4444-dddddddddddd"), "Placing own protection while climbing", true, "Traditional Climbing", "traditional-climbing", new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("dddddddd-4444-4444-4444-eeeeeeeeeeee"), "Climbing frozen waterfalls and ice formations", true, "Ice Climbing", "ice-climbing", new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("dddddddd-4444-4444-4444-ffffffffffff"), "Mountain climbing expeditions", true, "Mountaineering", "mountaineering", new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("eeeeeeee-5555-5555-5555-111111111111"), "Skiing in unpatrolled areas", true, "Backcountry Skiing", "backcountry-skiing", new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("eeeeeeee-5555-5555-5555-aaaaaaaaaaaa"), "Alpine skiing on slopes", true, "Downhill Skiing", "downhill-skiing", new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("eeeeeeee-5555-5555-5555-bbbbbbbbbbbb"), "Nordic skiing on trails", true, "Cross-Country Skiing", "cross-country-skiing", new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("eeeeeeee-5555-5555-5555-cccccccccccc"), "Riding snow on a board", true, "Snowboarding", "snowboarding", new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("eeeeeeee-5555-5555-5555-dddddddddddd"), "Walking on snow with specialized footwear", true, "Snowshoeing", "snowshoeing", new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("eeeeeeee-5555-5555-5555-eeeeeeeeeeee"), "Skating on frozen surfaces", true, "Ice Skating", "ice-skating", new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("eeeeeeee-5555-5555-5555-ffffffffffff"), "Riding motorized snow vehicles", true, "Snowmobiling", "snowmobiling", new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("ffffffff-6666-6666-6666-111111111111"), "Multi-day cycling trips with luggage", true, "Bike Touring", "bike-touring", new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("ffffffff-6666-6666-6666-aaaaaaaaaaaa"), "Off-road cycling on trails", true, "Mountain Biking", "mountain-biking", new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("ffffffff-6666-6666-6666-bbbbbbbbbbbb"), "Cycling on paved roads", true, "Road Cycling", "road-cycling", new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("ffffffff-6666-6666-6666-cccccccccccc"), "Cycling on gravel roads", true, "Gravel Cycling", "gravel-cycling", new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("ffffffff-6666-6666-6666-dddddddddddd"), "Bicycle motocross", true, "BMX", "bmx", new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("ffffffff-6666-6666-6666-eeeeeeeeeeee"), "Mixed terrain cycling with obstacles", true, "Cyclocross", "cyclocross", new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("ffffffff-6666-6666-6666-ffffffffffff"), "Cycling with oversized tires on soft surfaces", true, "Fat Biking", "fat-biking", new Guid("66666666-6666-6666-6666-666666666666") }
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
