using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pamaxie.Database.Native.Migrations
{
    public partial class RemovedParametersUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "has_closed_access",
                table: "users");

            migrationBuilder.DropColumn(
                name: "using_closed_access",
                table: "users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "has_closed_access",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "using_closed_access",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
