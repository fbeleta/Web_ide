using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebIde.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    OIB = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    JMBG = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DomainUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GitHubId = table.Column<string>(type: "text", nullable: true),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Problems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Difficulty = table.Column<int>(type: "integer", nullable: false),
                    TimeLimitMs = table.Column<int>(type: "integer", nullable: false),
                    MemoryLimitKb = table.Column<int>(type: "integer", nullable: false),
                    FloatTolerance = table.Column<double>(type: "double precision", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AuthorUsername = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Problems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationMembers",
                columns: table => new
                {
                    MembersId = table.Column<int>(type: "integer", nullable: false),
                    OrganizationsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationMembers", x => new { x.MembersId, x.OrganizationsId });
                    table.ForeignKey(
                        name: "FK_OrganizationMembers_DomainUsers_MembersId",
                        column: x => x.MembersId,
                        principalTable: "DomainUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationMembers_Organizations_OrganizationsId",
                        column: x => x.OrganizationsId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProblemSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProblemSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProblemSets_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    StoredFileName = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProblemId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachments_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestCases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InputArgs = table.Column<string>(type: "text", nullable: false),
                    ExpectedOutput = table.Column<string>(type: "text", nullable: false),
                    IsSample = table.Column<bool>(type: "boolean", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProblemId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestCases_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProblemTags",
                columns: table => new
                {
                    ProblemsId = table.Column<int>(type: "integer", nullable: false),
                    TagsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProblemTags", x => new { x.ProblemsId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_ProblemTags_Problems_ProblemsId",
                        column: x => x.ProblemsId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProblemTags_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProblemSetProblems",
                columns: table => new
                {
                    ProblemSetId = table.Column<int>(type: "integer", nullable: false),
                    ProblemsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProblemSetProblems", x => new { x.ProblemSetId, x.ProblemsId });
                    table.ForeignKey(
                        name: "FK_ProblemSetProblems_ProblemSets_ProblemSetId",
                        column: x => x.ProblemSetId,
                        principalTable: "ProblemSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProblemSetProblems_Problems_ProblemsId",
                        column: x => x.ProblemsId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExecutionResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubmissionId = table.Column<int>(type: "integer", nullable: false),
                    TestCaseId = table.Column<int>(type: "integer", nullable: false),
                    Stdout = table.Column<string>(type: "text", nullable: false),
                    Stderr = table.Column<string>(type: "text", nullable: false),
                    ExitCode = table.Column<int>(type: "integer", nullable: false),
                    WallTimeMs = table.Column<int>(type: "integer", nullable: false),
                    PeakMemoryKb = table.Column<int>(type: "integer", nullable: false),
                    Verdict = table.Column<int>(type: "integer", nullable: false),
                    TimedOut = table.Column<bool>(type: "boolean", nullable: false),
                    MemoryExceeded = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecutionResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExecutionResults_TestCases_TestCaseId",
                        column: x => x.TestCaseId,
                        principalTable: "TestCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceCode = table.Column<string>(type: "text", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    WallTimeMs = table.Column<int>(type: "integer", nullable: false),
                    PeakMemoryKb = table.Column<int>(type: "integer", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ProblemId = table.Column<int>(type: "integer", nullable: false),
                    ExecutionResultId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Submissions_DomainUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "DomainUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Submissions_ExecutionResults_ExecutionResultId",
                        column: x => x.ExecutionResultId,
                        principalTable: "ExecutionResults",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Submissions_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DomainUsers",
                columns: new[] { "Id", "AvatarUrl", "DeletedAt", "DisplayName", "Email", "GitHubId", "RegisteredAt", "Role", "Username" },
                values: new object[,]
                {
                    { 1, null, null, "Ana Kovač", "ana@example.com", null, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "ana_k" },
                    { 2, null, null, "Mario Blažić", "mario@example.com", null, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "mario_b" },
                    { 3, null, null, "Prof. Horvat", "prof@example.com", null, new DateTime(2024, 6, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, "prof_hr" },
                    { 4, null, null, "Admin", "admin@webide.io", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, "admin" }
                });

            migrationBuilder.InsertData(
                table: "Organizations",
                columns: new[] { "Id", "DeletedAt", "Description", "Name" },
                values: new object[,]
                {
                    { 1, null, "Faculty of Electrical Engineering and Computing — Algorithms course. Weekly problem sets and graded contests.", "FER Algorithms" },
                    { 2, null, "Student-run open source club. Practice problems, hackathons, and community challenges.", "Open Source Club" }
                });

            migrationBuilder.InsertData(
                table: "Problems",
                columns: new[] { "Id", "AuthorUsername", "CreatedAt", "DeletedAt", "Description", "Difficulty", "FloatTolerance", "MemoryLimitKb", "TimeLimitMs", "Title" },
                values: new object[,]
                {
                    { 1, "admin", new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, "Given an array of integers nums and an integer target, return indices of the two numbers that add up to target.", 0, null, 65536, 1000, "Two Sum" },
                    { 2, "admin", new DateTime(2025, 2, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, "Given a string s, find the length of the longest substring without duplicate characters.", 1, null, 65536, 2000, "Longest Substring Without Repeating Characters" },
                    { 3, "admin", new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, "Given two sorted arrays nums1 and nums2, return the median of the two sorted arrays. The overall run time complexity should be O(log(m+n)).", 2, null, 131072, 3000, "Median of Two Sorted Arrays" },
                    { 4, "prof_hr", new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "You are climbing a staircase. It takes n steps to reach the top. Each time you can climb 1 or 2 steps. In how many distinct ways can you climb to the top?", 0, null, 32768, 1000, "Climbing Stairs" },
                    { 5, "prof_hr", new DateTime(2025, 5, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, "There are numCourses courses you have to take. Some courses have prerequisites. Given the total number of courses and a list of prerequisite pairs, determine if it is possible to finish all courses.", 1, null, 65536, 2000, "Course Schedule" }
                });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "DeletedAt", "Name" },
                values: new object[,]
                {
                    { 1, null, "Arrays" },
                    { 2, null, "Hash Map" },
                    { 3, null, "Sliding Window" },
                    { 4, null, "Binary Search" },
                    { 5, null, "Dynamic Programming" },
                    { 6, null, "Graph" }
                });

            migrationBuilder.InsertData(
                table: "OrganizationMembers",
                columns: new[] { "MembersId", "OrganizationsId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 2 }
                });

            migrationBuilder.InsertData(
                table: "ProblemSets",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "Description", "IsPublic", "OrderIndex", "OrganizationId", "Title" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Array manipulation and hashing basics.", true, 1, 1, "Week 1 — Fundamentals" },
                    { 2, new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Dynamic programming, graph traversal, and advanced search techniques.", false, 2, 1, "Advanced Algorithms" },
                    { 3, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Quick 2-hour sprint with easy and medium problems. Open to all club members.", true, 1, 2, "OSC Sprint #1" }
                });

            migrationBuilder.InsertData(
                table: "ProblemTags",
                columns: new[] { "ProblemsId", "TagsId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 2, 2 },
                    { 2, 3 },
                    { 3, 4 },
                    { 4, 5 },
                    { 5, 6 }
                });

            migrationBuilder.InsertData(
                table: "Submissions",
                columns: new[] { "Id", "DeletedAt", "ExecutionResultId", "Language", "PeakMemoryKb", "ProblemId", "Score", "SourceCode", "Status", "SubmittedAt", "UserId", "WallTimeMs" },
                values: new object[,]
                {
                    { 1, null, null, "cpp", 2048, 1, 100, "// correct two sum", 2, new DateTime(2026, 5, 6, 23, 55, 0, 0, DateTimeKind.Utc), 1, 45 },
                    { 2, null, null, "cpp", 4096, 2, 100, "// sliding window", 2, new DateTime(2026, 5, 6, 23, 50, 0, 0, DateTimeKind.Utc), 1, 120 },
                    { 3, null, null, "cpp", 8192, 3, 60, "// binary search attempt", 3, new DateTime(2026, 5, 6, 23, 45, 0, 0, DateTimeKind.Utc), 1, 200 },
                    { 4, null, null, "cpp", 2048, 1, 100, "// brute force O(n^2)", 2, new DateTime(2026, 5, 6, 23, 40, 0, 0, DateTimeKind.Utc), 2, 980 },
                    { 5, null, null, "cpp", 4096, 2, 0, "// naive TLE", 4, new DateTime(2026, 5, 6, 23, 35, 0, 0, DateTimeKind.Utc), 2, 2001 },
                    { 6, null, null, "cpp", 4096, 3, 100, "// correct binary search", 2, new DateTime(2026, 5, 6, 23, 30, 0, 0, DateTimeKind.Utc), 2, 55 },
                    { 7, null, null, "cpp", 1024, 1, 100, "// optimal hash map", 2, new DateTime(2026, 5, 6, 23, 25, 0, 0, DateTimeKind.Utc), 3, 32 },
                    { 8, null, null, "cpp", 0, 3, 0, "// compile error", 6, new DateTime(2026, 5, 6, 23, 20, 0, 0, DateTimeKind.Utc), 3, 0 }
                });

            migrationBuilder.InsertData(
                table: "TestCases",
                columns: new[] { "Id", "DeletedAt", "ExpectedOutput", "InputArgs", "IsSample", "OrderIndex", "Points", "ProblemId" },
                values: new object[,]
                {
                    { 1, null, "[0,1]", "[2,7,11,15], 9", true, 1, 30, 1 },
                    { 2, null, "[1,2]", "[3,2,4], 6", true, 2, 30, 1 },
                    { 3, null, "[0,1]", "[3,3], 6", false, 3, 40, 1 },
                    { 4, null, "3", "\"abcabcbb\"", true, 1, 33, 2 },
                    { 5, null, "1", "\"bbbbb\"", true, 2, 33, 2 },
                    { 6, null, "3", "\"pwwkew\"", false, 3, 34, 2 },
                    { 7, null, "2.0", "[1,3], [2]", true, 1, 25, 3 },
                    { 8, null, "2.5", "[1,2], [3,4]", true, 2, 25, 3 },
                    { 9, null, "0.0", "[0,0], [0,0]", false, 3, 25, 3 },
                    { 10, null, "1.0", "[], [1]", false, 4, 25, 3 },
                    { 11, null, "2", "2", true, 1, 50, 4 },
                    { 12, null, "3", "3", true, 2, 50, 4 },
                    { 13, null, "true", "2, [[1,0]]", true, 1, 50, 5 },
                    { 14, null, "false", "2, [[1,0],[0,1]]", true, 2, 50, 5 }
                });

            migrationBuilder.InsertData(
                table: "ProblemSetProblems",
                columns: new[] { "ProblemSetId", "ProblemsId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 1, 3 },
                    { 2, 4 },
                    { 2, 5 },
                    { 3, 1 },
                    { 3, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_ProblemId",
                table: "Attachments",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionResults_TestCaseId",
                table: "ExecutionResults",
                column: "TestCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMembers_OrganizationsId",
                table: "OrganizationMembers",
                column: "OrganizationsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProblemSetProblems_ProblemsId",
                table: "ProblemSetProblems",
                column: "ProblemsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProblemSets_OrganizationId",
                table: "ProblemSets",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProblemTags_TagsId",
                table: "ProblemTags",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ExecutionResultId",
                table: "Submissions",
                column: "ExecutionResultId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ProblemId",
                table: "Submissions",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_UserId",
                table: "Submissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TestCases_ProblemId",
                table: "TestCases",
                column: "ProblemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "OrganizationMembers");

            migrationBuilder.DropTable(
                name: "ProblemSetProblems");

            migrationBuilder.DropTable(
                name: "ProblemTags");

            migrationBuilder.DropTable(
                name: "Submissions");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ProblemSets");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "DomainUsers");

            migrationBuilder.DropTable(
                name: "ExecutionResults");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropTable(
                name: "TestCases");

            migrationBuilder.DropTable(
                name: "Problems");
        }
    }
}
