using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_notch_stickers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "opt_notchordersticker",
                columns: table => new
                {
                    NotchOrderStickerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NotchOrderId = table.Column<int>(type: "int", nullable: false),
                    SemiproductId = table.Column<int>(type: "int", nullable: false),
                    IsAssembled = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opt_notchordersticker", x => x.NotchOrderStickerId);
                    table.ForeignKey(
                        name: "Opt_NotchOrderSticker_NotchOrderId",
                        column: x => x.NotchOrderId,
                        principalTable: "opt_notchorder",
                        principalColumn: "NotchOrderId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_opt_notchordersticker_NotchOrderId",
                table: "opt_notchordersticker",
                column: "NotchOrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "opt_notchordersticker");
        }
    }
}
