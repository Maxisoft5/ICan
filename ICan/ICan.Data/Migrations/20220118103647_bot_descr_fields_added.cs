using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class bot_descr_fields_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BotDescription",
                table: "opt_product",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "BotInformation",
                table: "opt_product",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BotDescription",
                table: "opt_product");

            migrationBuilder.DropColumn(
                name: "BotInformation",
                table: "opt_product");
        }
    }
}
