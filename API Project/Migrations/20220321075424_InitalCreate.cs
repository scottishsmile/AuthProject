using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace API_Project.Migrations
{
    public partial class InitalCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    EmailVerifyToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateEmailVerifySent = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordHash = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PasswordSalt = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "basic"),
                    LoginTries = table.Column<int>(type: "int", nullable: false),
                    AccountLocked = table.Column<bool>(type: "bit", nullable: false),
                    DateLocked = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ForgotPass = table.Column<bool>(type: "bit", nullable: false),
                    DatePassResetEmailSent = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResetToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Newsletter = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccountLocked", "DateEmailVerifySent", "DateLocked", "DatePassResetEmailSent", "Email", "EmailVerified", "EmailVerifyToken", "ForgotPass", "LoginTries", "Newsletter", "PasswordHash", "PasswordSalt", "ResetToken", "Role", "Username" },
                values: new object[] { 1, false, new DateTime(2022, 3, 21, 7, 54, 24, 74, DateTimeKind.Utc).AddTicks(7941), new DateTime(2022, 3, 21, 7, 54, 24, 74, DateTimeKind.Utc).AddTicks(9673), new DateTime(2022, 3, 21, 7, 54, 24, 74, DateTimeKind.Utc).AddTicks(6692), "admin@admin.com", true, null, false, 0, false, new byte[] { 19, 81, 84, 123, 251, 77, 122, 194, 45, 110, 235, 113, 199, 154, 245, 67, 230, 34, 168, 152, 222, 114, 161, 221, 79, 61, 87, 50, 142, 165, 2, 173, 200, 118, 74, 100, 112, 34, 218, 12, 149, 98, 149, 48, 236, 117, 78, 88, 64, 61, 127, 25, 134, 203, 29, 1, 32, 153, 201, 229, 28, 183, 101, 80 }, new byte[] { 160, 231, 159, 116, 50, 214, 97, 64, 72, 16, 214, 73, 56, 209, 35, 228, 30, 150, 76, 132, 192, 140, 174, 89, 36, 99, 248, 14, 115, 116, 186, 48, 92, 28, 201, 186, 255, 88, 227, 235, 7, 192, 167, 220, 223, 104, 114, 202, 33, 74, 204, 178, 216, 30, 169, 167, 157, 207, 40, 37, 111, 242, 141, 38, 253, 138, 197, 224, 210, 112, 132, 163, 138, 77, 155, 116, 170, 174, 68, 236, 113, 148, 48, 201, 220, 85, 168, 200, 45, 35, 201, 212, 3, 211, 194, 116, 188, 241, 189, 142, 88, 148, 170, 47, 185, 90, 151, 50, 118, 80, 19, 111, 131, 207, 135, 181, 132, 206, 95, 192, 249, 167, 78, 132, 198, 238, 52, 208 }, null, "admin", "admin" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccountLocked", "DateEmailVerifySent", "DateLocked", "DatePassResetEmailSent", "Email", "EmailVerified", "EmailVerifyToken", "ForgotPass", "LoginTries", "Newsletter", "PasswordHash", "PasswordSalt", "ResetToken", "Role", "Username" },
                values: new object[] { 2, false, new DateTime(2022, 3, 21, 7, 54, 24, 75, DateTimeKind.Utc).AddTicks(132), new DateTime(2022, 3, 21, 7, 54, 24, 75, DateTimeKind.Utc).AddTicks(134), new DateTime(2022, 3, 21, 7, 54, 24, 75, DateTimeKind.Utc).AddTicks(128), "basic_test@test1222.com", true, null, false, 0, false, new byte[] { 19, 81, 84, 123, 251, 77, 122, 194, 45, 110, 235, 113, 199, 154, 245, 67, 230, 34, 168, 152, 222, 114, 161, 221, 79, 61, 87, 50, 142, 165, 2, 173, 200, 118, 74, 100, 112, 34, 218, 12, 149, 98, 149, 48, 236, 117, 78, 88, 64, 61, 127, 25, 134, 203, 29, 1, 32, 153, 201, 229, 28, 183, 101, 80 }, new byte[] { 160, 231, 159, 116, 50, 214, 97, 64, 72, 16, 214, 73, 56, 209, 35, 228, 30, 150, 76, 132, 192, 140, 174, 89, 36, 99, 248, 14, 115, 116, 186, 48, 92, 28, 201, 186, 255, 88, 227, 235, 7, 192, 167, 220, 223, 104, 114, 202, 33, 74, 204, 178, 216, 30, 169, 167, 157, 207, 40, 37, 111, 242, 141, 38, 253, 138, 197, 224, 210, 112, 132, 163, 138, 77, 155, 116, 170, 174, 68, 236, 113, 148, 48, 201, 220, 85, 168, 200, 45, 35, 201, 212, 3, 211, 194, 116, 188, 241, 189, 142, 88, 148, 170, 47, 185, 90, 151, 50, 118, 80, 19, 111, 131, 207, 135, 181, 132, 206, 95, 192, 249, 167, 78, 132, 198, 238, 52, 208 }, null, "basic", "basic_test" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccountLocked", "DateEmailVerifySent", "DateLocked", "DatePassResetEmailSent", "Email", "EmailVerified", "EmailVerifyToken", "ForgotPass", "LoginTries", "Newsletter", "PasswordHash", "PasswordSalt", "ResetToken", "Role", "Username" },
                values: new object[] { 3, false, new DateTime(2022, 3, 21, 7, 54, 24, 75, DateTimeKind.Utc).AddTicks(136), new DateTime(2022, 3, 21, 7, 54, 24, 75, DateTimeKind.Utc).AddTicks(138), new DateTime(2022, 3, 21, 7, 54, 24, 75, DateTimeKind.Utc).AddTicks(135), "premium_test@test122.com", true, null, false, 0, false, new byte[] { 19, 81, 84, 123, 251, 77, 122, 194, 45, 110, 235, 113, 199, 154, 245, 67, 230, 34, 168, 152, 222, 114, 161, 221, 79, 61, 87, 50, 142, 165, 2, 173, 200, 118, 74, 100, 112, 34, 218, 12, 149, 98, 149, 48, 236, 117, 78, 88, 64, 61, 127, 25, 134, 203, 29, 1, 32, 153, 201, 229, 28, 183, 101, 80 }, new byte[] { 160, 231, 159, 116, 50, 214, 97, 64, 72, 16, 214, 73, 56, 209, 35, 228, 30, 150, 76, 132, 192, 140, 174, 89, 36, 99, 248, 14, 115, 116, 186, 48, 92, 28, 201, 186, 255, 88, 227, 235, 7, 192, 167, 220, 223, 104, 114, 202, 33, 74, 204, 178, 216, 30, 169, 167, 157, 207, 40, 37, 111, 242, 141, 38, 253, 138, 197, 224, 210, 112, 132, 163, 138, 77, 155, 116, 170, 174, 68, 236, 113, 148, 48, 201, 220, 85, 168, 200, 45, 35, 201, 212, 3, 211, 194, 116, 188, 241, 189, 142, 88, 148, 170, 47, 185, 90, 151, 50, 118, 80, 19, 111, 131, 207, 135, 181, 132, 206, 95, 192, 249, 167, 78, 132, 198, 238, 52, 208 }, null, "premium", "premium_test" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
