using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_movePaper_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "opt_movepaper",
                columns: table => new
                {
                    MovePaperId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MoveDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SenderWarehouseId = table.Column<int>(type: "int", nullable: false),
                    ReceiverWarehouseId = table.Column<int>(type: "int", nullable: false),
                    SheetCount = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<double>(type: "double", nullable: false),
                    PaperId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opt_movepaper", x => x.MovePaperId);
                    table.ForeignKey(
                        name: "FK_opt_movepaper_opt_paper_PaperId",
                        column: x => x.PaperId,
                        principalTable: "opt_paper",
                        principalColumn: "PaperID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_opt_movepaper_PaperId",
                table: "opt_movepaper",
                column: "PaperId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "opt_movepaper");
        }
    }
}
