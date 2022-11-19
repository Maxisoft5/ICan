using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class remove_market_reviews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "opt_marketplaceproductreview");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "opt_marketplaceproductreview",
                columns: table => new
                {
                    MarketplaceProductReviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AuthorName = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    MarketplaceProductId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opt_marketplaceproductreview", x => x.MarketplaceProductReviewId);
                    table.ForeignKey(
                        name: "Opt_MarketplaceProductReview_MarketplaceProductId",
                        column: x => x.MarketplaceProductId,
                        principalTable: "opt_marketplaceproduct",
                        principalColumn: "MarketplaceProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_opt_marketplaceproductreview_MarketplaceProductId",
                table: "opt_marketplaceproductreview",
                column: "MarketplaceProductId");
        }
    }
}
