using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class Add_type_of_paper_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TypeOfPaperId",
                table: "opt_paper",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "opt_typesofpaper",
                columns: table => new
                {
                    TypeOfPaperId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opt_typesofpaper", x => x.TypeOfPaperId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_opt_paper_TypeOfPaperId",
                table: "opt_paper",
                column: "TypeOfPaperId");

            migrationBuilder.AddForeignKey(
                name: "Opt_TypeOfPaper_TypeOfPaperId",
                table: "opt_paper",
                column: "TypeOfPaperId",
                principalTable: "opt_typesofpaper",
                principalColumn: "TypeOfPaperId",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Opt_TypeOfPaper_TypeOfPaperId",
                table: "opt_paper");

            migrationBuilder.DropTable(
                name: "opt_typesofpaper");

            migrationBuilder.DropIndex(
                name: "IX_opt_paper_TypeOfPaperId",
                table: "opt_paper");

            migrationBuilder.DropColumn(
                name: "TypeOfPaperId",
                table: "opt_paper");
        }
    }
}
