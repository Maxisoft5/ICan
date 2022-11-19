using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class remove_clientId_from_shop : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Opt_Shop_ClientId",
                table: "opt_shop");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "opt_shop");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                table: "opt_shop",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "Opt_Shop_ClientId",
                table: "opt_shop",
                column: "ClientId",
                principalTable: "aspnetusers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
