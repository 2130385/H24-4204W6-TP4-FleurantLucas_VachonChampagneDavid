using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostHubAPI.Migrations
{
    public partial class migrationSeedModerator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "ce22f252-b06a-4149-93c4-ef42a80c1fcc", "df11cc11-8de3-491c-90ee-508332fd0b11", "Moderator", "MODERATOR" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FileName", "LockoutEnabled", "LockoutEnd", "MimeType", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "4e743245-6931-4d50-b3d0-597d9bd9dba2", 0, "355d6e09-f1ee-4f01-834c-e80d3e458a47", "moderator@gmail.com", false, null, false, null, null, "MODERATOR@GMAIL.COM", "MODERATOR", "AQAAAAEAACcQAAAAEIv/JZ3BzftZQo/+n5/IJYgYbMAs501SBi7LUZGZf/tJLHhm67n4rt9FQ+I475e5fA==", null, false, "04e3b845-72f3-49f2-9d6c-b942c001ea7c", false, "moderator" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "ce22f252-b06a-4149-93c4-ef42a80c1fcc", "4e743245-6931-4d50-b3d0-597d9bd9dba2" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "ce22f252-b06a-4149-93c4-ef42a80c1fcc", "4e743245-6931-4d50-b3d0-597d9bd9dba2" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ce22f252-b06a-4149-93c4-ef42a80c1fcc");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "4e743245-6931-4d50-b3d0-597d9bd9dba2");
        }
    }
}
