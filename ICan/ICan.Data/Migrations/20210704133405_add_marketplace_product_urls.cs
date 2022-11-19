using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_marketplace_product_urls : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "opt_marketplaceproducturl",
				columns: table => new
				{
					MarketplaceProductUrlId = table.Column<int>(type: "int", nullable: false)
						.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
					MarketplaceProductId = table.Column<int>(type: "int", nullable: false),
					Url = table.Column<string>(type: "longtext", nullable: true)
						.Annotation("MySql:CharSet", "utf8mb4")
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_opt_marketplaceproducturl", x => x.MarketplaceProductUrlId);
					table.ForeignKey(
						name: "Opt_MarketplaceProductUrl_MarketplaceProduct",
						column: x => x.MarketplaceProductId,
						principalTable: "opt_marketplaceproduct",
						principalColumn: "MarketplaceProductId",
						onDelete: ReferentialAction.Cascade);
				})
				.Annotation("MySql:CharSet", "utf8mb4");

			migrationBuilder.CreateIndex(
				name: "IX_opt_marketplaceproducturl_MarketplaceProductId",
				table: "opt_marketplaceproducturl",
				column: "MarketplaceProductId");
			
			migrationBuilder.Sql(@"INSERT INTO opt_marketplaceproducturl (MarketplaceProductId, Url)
                            SELECT MarketplaceProductId, Url 
							FROM opt_marketplaceproducturl ");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "opt_marketplaceproducturl");
		}
	}
}
