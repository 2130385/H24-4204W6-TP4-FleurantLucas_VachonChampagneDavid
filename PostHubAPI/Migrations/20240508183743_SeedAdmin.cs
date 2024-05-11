using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostHubAPI.Migrations
{
    public partial class SeedAdmin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1c213ed1-b8ff-4b4e-97fa-e4caf73e619f", "85d2659f-6b4b-49e2-9aa3-b12f10f4a2a9", "Moderator", "MODERATOR" },
                    { "7292a77c-ded5-4d49-9c2a-3d241f33a261", "e764dafd-12c3-40d7-9891-7dd591bf3793", "Administrator", "ADMINISTRATOR" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FileName", "LockoutEnabled", "LockoutEnd", "MimeType", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "48a56c88-e863-4f19-9f1f-c14536cec605", 0, "0cd9b01a-333b-4f6b-bbda-29678e26967c", "ADMIN@gmail.com", false, null, false, null, null, "ADMIN@GMAIL.COM", "ADMIN", "AQAAAAEAACcQAAAAEJ+nuTz/9Ua5XJM70pKSN+VKGkwu+vMhXbbCLklH9wpTj7xfe3It9ciXXbwGg8VzCQ==", null, false, "e90b84f2-16de-45c9-8416-dd6a2b8374ab", false, "admin" },
                    { "4afbe2f2-2fe0-48c2-a895-a22b509670f3", 0, "1d44363c-379d-4afc-97a0-1f804a9e194b", "moderator@gmail.com", false, null, false, null, null, "MODERATOR@GMAIL.COM", "MODERATOR", "AQAAAAEAACcQAAAAEAtMZcx6D9z+9zjhdggvlp1DNG9WQ5Zxf2jpy2cD3aecIAEPcDKt5+aUbbf3fDbeHw==", null, false, "7bfe88a3-78f5-4979-bd1f-fe2f79d14a68", false, "moderator" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1c213ed1-b8ff-4b4e-97fa-e4caf73e619f", "4afbe2f2-2fe0-48c2-a895-a22b509670f3" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7292a77c-ded5-4d49-9c2a-3d241f33a261");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1c213ed1-b8ff-4b4e-97fa-e4caf73e619f", "4afbe2f2-2fe0-48c2-a895-a22b509670f3" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "48a56c88-e863-4f19-9f1f-c14536cec605");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1c213ed1-b8ff-4b4e-97fa-e4caf73e619f");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "4afbe2f2-2fe0-48c2-a895-a22b509670f3");

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
    }
}
