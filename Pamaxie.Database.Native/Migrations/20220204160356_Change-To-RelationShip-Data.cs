using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pamaxie.Database.Native.Migrations
{
    public partial class ChangeToRelationShipData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_project_users_project_id_user_id",
                table: "project_users");

            migrationBuilder.DropIndex(
                name: "ix_project_users_user_id_project_id",
                table: "project_users");

            migrationBuilder.DropIndex(
                name: "ix_api_keys_owner_id",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "api_key_type",
                table: "api_keys");

            migrationBuilder.RenameColumn(
                name: "owner_id",
                table: "api_keys",
                newName: "id");

            migrationBuilder.AlterColumn<decimal>(
                name: "user_id",
                table: "two_factor_users",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AddColumn<decimal>(
                name: "id",
                table: "two_factor_users",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ttl",
                table: "two_factor_users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "user_id",
                table: "project_users",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "project_id",
                table: "project_users",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AddColumn<decimal>(
                name: "id",
                table: "project_users",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ttl",
                table: "project_users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "user_id",
                table: "known_user_ips",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AddColumn<decimal>(
                name: "id",
                table: "known_user_ips",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ttl",
                table: "known_user_ips",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "project_id",
                table: "api_keys",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ttl",
                table: "api_keys",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_two_factor_users",
                table: "two_factor_users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_project_users",
                table: "project_users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_known_user_ips",
                table: "known_user_ips",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_api_keys",
                table: "api_keys",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_project_users_project_id",
                table: "project_users",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_users_user_id",
                table: "project_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_api_keys_project_id",
                table: "api_keys",
                column: "project_id");

            migrationBuilder.AddForeignKey(
                name: "fk_api_keys_projects_project_id",
                table: "api_keys",
                column: "project_id",
                principalTable: "projects",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_known_user_ips_users_user_id",
                table: "known_user_ips",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_project_users_projects_project_id",
                table: "project_users",
                column: "project_id",
                principalTable: "projects",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_project_users_users_user_id",
                table: "project_users",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_two_factor_users_users_user_id",
                table: "two_factor_users",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_api_keys_projects_project_id",
                table: "api_keys");

            migrationBuilder.DropForeignKey(
                name: "fk_known_user_ips_users_user_id",
                table: "known_user_ips");

            migrationBuilder.DropForeignKey(
                name: "fk_project_users_projects_project_id",
                table: "project_users");

            migrationBuilder.DropForeignKey(
                name: "fk_project_users_users_user_id",
                table: "project_users");

            migrationBuilder.DropForeignKey(
                name: "fk_two_factor_users_users_user_id",
                table: "two_factor_users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_two_factor_users",
                table: "two_factor_users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_project_users",
                table: "project_users");

            migrationBuilder.DropIndex(
                name: "ix_project_users_project_id",
                table: "project_users");

            migrationBuilder.DropIndex(
                name: "ix_project_users_user_id",
                table: "project_users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_known_user_ips",
                table: "known_user_ips");

            migrationBuilder.DropPrimaryKey(
                name: "pk_api_keys",
                table: "api_keys");

            migrationBuilder.DropIndex(
                name: "ix_api_keys_project_id",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "id",
                table: "two_factor_users");

            migrationBuilder.DropColumn(
                name: "ttl",
                table: "two_factor_users");

            migrationBuilder.DropColumn(
                name: "id",
                table: "project_users");

            migrationBuilder.DropColumn(
                name: "ttl",
                table: "project_users");

            migrationBuilder.DropColumn(
                name: "id",
                table: "known_user_ips");

            migrationBuilder.DropColumn(
                name: "ttl",
                table: "known_user_ips");

            migrationBuilder.DropColumn(
                name: "project_id",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "ttl",
                table: "api_keys");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "api_keys",
                newName: "owner_id");

            migrationBuilder.AlterColumn<decimal>(
                name: "user_id",
                table: "two_factor_users",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "user_id",
                table: "project_users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "project_id",
                table: "project_users",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "user_id",
                table: "known_user_ips",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "api_key_type",
                table: "api_keys",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_project_users_project_id_user_id",
                table: "project_users",
                columns: new[] { "project_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "ix_project_users_user_id_project_id",
                table: "project_users",
                columns: new[] { "user_id", "project_id" });

            migrationBuilder.CreateIndex(
                name: "ix_api_keys_owner_id",
                table: "api_keys",
                column: "owner_id");
        }
    }
}
