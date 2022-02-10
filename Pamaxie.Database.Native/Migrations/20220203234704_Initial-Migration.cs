using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pamaxie.Database.Native.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "api_keys",
                columns: table => new
                {
                    owner_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    credential_hash = table.Column<string>(type: "text", nullable: true),
                    api_key_type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "known_user_ips",
                columns: table => new
                {
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ip_address = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "project_users",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    project_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    permissions = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    owner_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    flags = table.Column<int>(type: "integer", nullable: false),
                    creation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ttl = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_projects", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "two_factor_users",
                columns: table => new
                {
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    public_key = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    username = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    first_name = table.Column<string>(type: "text", nullable: true),
                    last_name = table.Column<string>(type: "text", nullable: true),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    flags = table.Column<int>(type: "integer", nullable: false),
                    creation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ttl = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_api_keys_owner_id",
                table: "api_keys",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_known_user_ips_user_id",
                table: "known_user_ips",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_users_project_id_user_id",
                table: "project_users",
                columns: new[] { "project_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "ix_project_users_user_id_project_id",
                table: "project_users",
                columns: new[] { "user_id", "project_id" });

            migrationBuilder.CreateIndex(
                name: "ix_projects_ttl",
                table: "projects",
                column: "ttl");

            migrationBuilder.CreateIndex(
                name: "ix_two_factor_users_user_id",
                table: "two_factor_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_ttl",
                table: "users",
                column: "ttl");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "api_keys");

            migrationBuilder.DropTable(
                name: "known_user_ips");

            migrationBuilder.DropTable(
                name: "project_users");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "two_factor_users");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
