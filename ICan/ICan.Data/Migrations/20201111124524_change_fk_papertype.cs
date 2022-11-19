using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class change_fk_papertype : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Opt_TypeOfPaper_TypeOfPaperId",
                table: "opt_paper");

            migrationBuilder.AddForeignKey(
                name: "Opt_Paper_TypeOfPaperId",
                table: "opt_paper",
                column: "TypeOfPaperId",
                principalTable: "opt_typesofpaper",
                principalColumn: "TypeOfPaperId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Opt_Paper_TypeOfPaperId",
                table: "opt_paper");

            migrationBuilder.AddForeignKey(
                name: "Opt_TypeOfPaper_TypeOfPaperId",
                table: "opt_paper",
                column: "TypeOfPaperId",
                principalTable: "opt_typesofpaper",
                principalColumn: "TypeOfPaperId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
