using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class printorderpaper_change_fk_behaviour : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Opt_PrintOrderPaper_PrintOrderID",
                table: "opt_printorderpaper");

            migrationBuilder.AddForeignKey(
                name: "Opt_PrintOrderPaper_PrintOrderID",
                table: "opt_printorderpaper",
                column: "PrintOrderId",
                principalTable: "opt_printorder",
                principalColumn: "PrintOrderID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Opt_PrintOrderPaper_PrintOrderID",
                table: "opt_printorderpaper");

            migrationBuilder.AddForeignKey(
                name: "Opt_PrintOrderPaper_PrintOrderID",
                table: "opt_printorderpaper",
                column: "PrintOrderId",
                principalTable: "opt_printorder",
                principalColumn: "PrintOrderID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
