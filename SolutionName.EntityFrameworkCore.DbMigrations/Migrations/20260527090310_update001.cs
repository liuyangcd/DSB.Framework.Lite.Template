using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolutionName.EntityFrameworkCore.DbMigrations.Migrations
{
    /// <inheritdoc />
    public partial class update001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SystemPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ParentId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icon = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Remark = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Sort = table.Column<int>(type: "int", nullable: false),
                    EntityStatus = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreateDateAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdateDateAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemPermissions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SystemRolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RoleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PermissionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EntityStatus = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreateDateAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdateDateAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemRolePermissions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SystemRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Remark = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Sort = table.Column<int>(type: "int", nullable: false),
                    EntityStatus = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreateDateAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdateDateAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemRoles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SystemUploadFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SavePath = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SaveName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Extension = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EntityStatus = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreateDateAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdateDateAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemUploadFiles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SystemUserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RoleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EntityStatus = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreateDateAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdateDateAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemUserRoles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SystemUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Account = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Salt = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Birth = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Avatar = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Remark = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EntityStatus = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreateDateAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdateDateAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemUsers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SystemPermissions_Code",
                table: "SystemPermissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemPermissions_Sort_CreateDateAt",
                table: "SystemPermissions",
                columns: new[] { "Sort", "CreateDateAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_SystemRolePermissions_PermissionId",
                table: "SystemRolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemRolePermissions_RoleId_PermissionId",
                table: "SystemRolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemRoles_Code",
                table: "SystemRoles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemRoles_Sort_CreateDateAt",
                table: "SystemRoles",
                columns: new[] { "Sort", "CreateDateAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_SystemUserRoles_RoleId",
                table: "SystemUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemUserRoles_UserId_RoleId",
                table: "SystemUserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemUsers_Account",
                table: "SystemUsers",
                column: "Account",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemUsers_CreateDateAt",
                table: "SystemUsers",
                column: "CreateDateAt",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemPermissions");

            migrationBuilder.DropTable(
                name: "SystemRolePermissions");

            migrationBuilder.DropTable(
                name: "SystemRoles");

            migrationBuilder.DropTable(
                name: "SystemUploadFiles");

            migrationBuilder.DropTable(
                name: "SystemUserRoles");

            migrationBuilder.DropTable(
                name: "SystemUsers");
        }
    }
}
