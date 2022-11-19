using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class AddOptPrintOrderPaperToMovePaper : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrintOrderPaperId",
                table: "opt_movepaper",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_opt_movepaper_PrintOrderPaperId",
                table: "opt_movepaper",
                column: "PrintOrderPaperId");

            migrationBuilder.AddForeignKey(
                name: "FK_opt_movepaper_opt_printorderpaper_PrintOrderPaperId",
                table: "opt_movepaper",
                column: "PrintOrderPaperId",
                principalTable: "opt_printorderpaper",
                principalColumn: "PrintOrderPaperID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_opt_movepaper_opt_printorderpaper_PrintOrderPaperId",
                table: "opt_movepaper");

            migrationBuilder.DropIndex(
                name: "IX_opt_movepaper_PrintOrderPaperId",
                table: "opt_movepaper");

            migrationBuilder.DropColumn(
                name: "PrintOrderPaperId",
                table: "opt_movepaper");
        }
    }
}
