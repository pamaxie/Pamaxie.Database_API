using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pamaxie.Database.Native.Migrations
{
    public partial class FixedFirstName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "first_name",
                table: "users",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "first_name",
                table: "users");
        }
    }
}
