using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class addpaperorderincomings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "opt_paperorderincomings",
                columns: table => new
                {
                    PaperOrderIncomingId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PaperOrderId = table.Column<int>(nullable: false),
                    Amount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opt_paperorderincomings", x => x.PaperOrderIncomingId);
                    table.ForeignKey(
                        name: "Opt_PaperOrderIncomig_PaperOrderId",
                        column: x => x.PaperOrderId,
                        principalTable: "opt_paperorder",
                        principalColumn: "PaperOrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_opt_paperorderincomings_PaperOrderId",
                table: "opt_paperorderincomings",
                column: "PaperOrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "opt_paperorderincomings");
        }
    }
}
