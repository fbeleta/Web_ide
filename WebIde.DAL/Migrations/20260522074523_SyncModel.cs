using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebIde.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ExecutionResults",
                columns: new[] { "Id", "ExitCode", "MemoryExceeded", "Stderr", "Stdout", "TimedOut" },
                values: new object[,]
                {
                    { 1, 0, false, "", "[0,1]", false },
                    { 2, 0, false, "", "3", false },
                    { 3, 0, false, "", "2.0", false },
                    { 4, 0, false, "", "[0,1]", false },
                    { 5, 0, false, "", "", true },
                    { 6, 0, false, "", "2.5", false },
                    { 7, 0, false, "", "[0,1]", false },
                    { 8, 1, false, "", "", false }
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
                columns: new[] { "Id", "AuthorUsername", "CreatedAt", "DeletedAt", "Description", "Difficulty", "MemoryLimitKb", "TimeLimitMs", "Title" },
                values: new object[,]
                {
                    { 1, "admin", new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, "Given an array of integers nums and an integer target, return indices of the two numbers that add up to target.", 0, 65536, 1000, "Two Sum" },
                    { 2, "admin", new DateTime(2025, 2, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, "Given a string s, find the length of the longest substring without duplicate characters.", 1, 65536, 2000, "Longest Substring Without Repeating Characters" },
                    { 3, "admin", new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, "Given two sorted arrays nums1 and nums2, return the median of the two sorted arrays. The overall run time complexity should be O(log(m+n)).", 2, 131072, 3000, "Median of Two Sorted Arrays" },
                    { 4, "prof_hr", new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "You are climbing a staircase. It takes n steps to reach the top. Each time you can climb 1 or 2 steps. In how many distinct ways can you climb to the top?", 0, 32768, 1000, "Climbing Stairs" },
                    { 5, "prof_hr", new DateTime(2025, 5, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, "There are numCourses courses you have to take. Some courses have prerequisites. Given the total number of courses and a list of prerequisite pairs, determine if it is possible to finish all courses.", 1, 65536, 2000, "Course Schedule" }
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
                table: "Users",
                columns: new[] { "Id", "DeletedAt", "DisplayName", "Email", "RegisteredAt", "Role", "Username" },
                values: new object[,]
                {
                    { 1, null, "Ana Kovač", "ana@example.com", new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "ana_k" },
                    { 2, null, "Mario Blažić", "mario@example.com", new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "mario_b" },
                    { 3, null, "Prof. Horvat", "prof@example.com", new DateTime(2024, 6, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, "prof_hr" },
                    { 4, null, "Admin", "admin@webide.io", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, "admin" }
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
                    { 1, null, 1, "cpp", 2048, 1, 100, "// correct two sum", 2, new DateTime(2026, 5, 6, 23, 55, 0, 0, DateTimeKind.Utc), 1, 45 },
                    { 2, null, 2, "cpp", 4096, 2, 100, "// sliding window", 2, new DateTime(2026, 5, 6, 23, 50, 0, 0, DateTimeKind.Utc), 1, 120 },
                    { 3, null, 3, "cpp", 8192, 3, 60, "// binary search attempt", 3, new DateTime(2026, 5, 6, 23, 45, 0, 0, DateTimeKind.Utc), 1, 200 },
                    { 4, null, 4, "cpp", 2048, 1, 100, "// brute force O(n^2)", 2, new DateTime(2026, 5, 6, 23, 40, 0, 0, DateTimeKind.Utc), 2, 980 },
                    { 5, null, 5, "cpp", 4096, 2, 0, "// naive TLE", 4, new DateTime(2026, 5, 6, 23, 35, 0, 0, DateTimeKind.Utc), 2, 2001 },
                    { 6, null, 6, "cpp", 4096, 3, 100, "// correct binary search", 2, new DateTime(2026, 5, 6, 23, 30, 0, 0, DateTimeKind.Utc), 2, 55 },
                    { 7, null, 7, "cpp", 1024, 1, 100, "// optimal hash map", 2, new DateTime(2026, 5, 6, 23, 25, 0, 0, DateTimeKind.Utc), 3, 32 },
                    { 8, null, 8, "cpp", 0, 3, 0, "// compile error", 6, new DateTime(2026, 5, 6, 23, 20, 0, 0, DateTimeKind.Utc), 3, 0 }
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OrganizationMembers",
                keyColumns: new[] { "MembersId", "OrganizationsId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "OrganizationMembers",
                keyColumns: new[] { "MembersId", "OrganizationsId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "OrganizationMembers",
                keyColumns: new[] { "MembersId", "OrganizationsId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "OrganizationMembers",
                keyColumns: new[] { "MembersId", "OrganizationsId" },
                keyValues: new object[] { 3, 1 });

            migrationBuilder.DeleteData(
                table: "OrganizationMembers",
                keyColumns: new[] { "MembersId", "OrganizationsId" },
                keyValues: new object[] { 4, 2 });

            migrationBuilder.DeleteData(
                table: "ProblemSetProblems",
                keyColumns: new[] { "ProblemSetId", "ProblemsId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "ProblemSetProblems",
                keyColumns: new[] { "ProblemSetId", "ProblemsId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "ProblemSetProblems",
                keyColumns: new[] { "ProblemSetId", "ProblemsId" },
                keyValues: new object[] { 1, 3 });

            migrationBuilder.DeleteData(
                table: "ProblemSetProblems",
                keyColumns: new[] { "ProblemSetId", "ProblemsId" },
                keyValues: new object[] { 2, 4 });

            migrationBuilder.DeleteData(
                table: "ProblemSetProblems",
                keyColumns: new[] { "ProblemSetId", "ProblemsId" },
                keyValues: new object[] { 2, 5 });

            migrationBuilder.DeleteData(
                table: "ProblemSetProblems",
                keyColumns: new[] { "ProblemSetId", "ProblemsId" },
                keyValues: new object[] { 3, 1 });

            migrationBuilder.DeleteData(
                table: "ProblemSetProblems",
                keyColumns: new[] { "ProblemSetId", "ProblemsId" },
                keyValues: new object[] { 3, 4 });

            migrationBuilder.DeleteData(
                table: "ProblemTags",
                keyColumns: new[] { "ProblemsId", "TagsId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "ProblemTags",
                keyColumns: new[] { "ProblemsId", "TagsId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "ProblemTags",
                keyColumns: new[] { "ProblemsId", "TagsId" },
                keyValues: new object[] { 2, 2 });

            migrationBuilder.DeleteData(
                table: "ProblemTags",
                keyColumns: new[] { "ProblemsId", "TagsId" },
                keyValues: new object[] { 2, 3 });

            migrationBuilder.DeleteData(
                table: "ProblemTags",
                keyColumns: new[] { "ProblemsId", "TagsId" },
                keyValues: new object[] { 3, 4 });

            migrationBuilder.DeleteData(
                table: "ProblemTags",
                keyColumns: new[] { "ProblemsId", "TagsId" },
                keyValues: new object[] { 4, 5 });

            migrationBuilder.DeleteData(
                table: "ProblemTags",
                keyColumns: new[] { "ProblemsId", "TagsId" },
                keyValues: new object[] { 5, 6 });

            migrationBuilder.DeleteData(
                table: "Submissions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Submissions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Submissions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Submissions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Submissions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Submissions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Submissions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Submissions",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "TestCases",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TestCases",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TestCases",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TestCases",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TestCases",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "TestCases",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "TestCases",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "TestCases",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "TestCases",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "TestCases",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "TestCases",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "TestCases",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "TestCases",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "TestCases",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "ExecutionResults",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ExecutionResults",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ExecutionResults",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ExecutionResults",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ExecutionResults",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ExecutionResults",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ExecutionResults",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ExecutionResults",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ProblemSets",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ProblemSets",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ProblemSets",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Problems",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Problems",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Problems",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Problems",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Problems",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Organizations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Organizations",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
