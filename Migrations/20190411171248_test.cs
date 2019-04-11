using Microsoft.EntityFrameworkCore.Migrations;

namespace BaseApi.Migrations
{
    public partial class test : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nqscqcsame",
                table: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nqscqcsame",
                table: "Users",
                nullable: true);
        }
    }
}
