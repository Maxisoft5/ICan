using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_site_managment : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameTable(
				 name: "opt_tags", newName: "opt_tag");

			migrationBuilder.AddColumn<string>(
				name: "SiteName",
				table: "opt_productseries",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "Description",
				table: "opt_product",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "Information",
				table: "opt_product",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "LongDescription",
				table: "opt_product",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "SiteDisplayOrder",
				table: "opt_product",
				nullable: true);

			migrationBuilder.AddColumn<bool>(
				name: "SiteEnabled",
				table: "opt_product",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<string>(
				name: "SiteName",
				table: "opt_product",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "VideoFileName",
				table: "opt_product",
				nullable: true);

			migrationBuilder.CreateTable(
				name: "opt_marketplaceproduct",
				columns: table => new
				{
					MarketplaceProductId = table.Column<int>(nullable: false)
						.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
					ProductId = table.Column<int>(nullable: false),
					MarketplaceId = table.Column<int>(nullable: false),
					Url = table.Column<string>(nullable: true),
					Price = table.Column<decimal>(nullable: true),
					Raiting = table.Column<int>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_opt_marketplaceproduct", x => x.MarketplaceProductId);
					table.ForeignKey(
						name: "Opt_MarketplaceProduct_MarketplaceId",
						column: x => x.MarketplaceId,
						principalTable: "opt_marketplace",
						principalColumn: "MarketplaceId",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "Opt_MarketplaceProduct_ProductId",
						column: x => x.ProductId,
						principalTable: "opt_product",
						principalColumn: "ProductID",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "opt_productimage",
				columns: table => new
				{
					ProductImageId = table.Column<int>(nullable: false)
						.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
					ProductId = table.Column<int>(nullable: false),
					IsCover = table.Column<bool>(nullable: false),
					Order = table.Column<int>(nullable: false),
					FileName = table.Column<string>(nullable: true),
					ImageType = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_opt_productimage", x => x.ProductImageId);
					table.ForeignKey(
						name: "Opt_ProductImage_ProductId",
						column: x => x.ProductId,
						principalTable: "opt_product",
						principalColumn: "ProductID",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "opt_producttag",
				columns: table => new
				{
					ProductTagId = table.Column<int>(nullable: false)
						.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
					TagId = table.Column<int>(nullable: false),
					ProductId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_opt_producttag", x => x.ProductTagId);
					table.ForeignKey(
						name: "Opt_ProductTag_ProductId",
						column: x => x.ProductId,
						principalTable: "opt_product",
						principalColumn: "ProductID",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "Opt_ProductTag_TagId",
						column: x => x.TagId,
						principalTable: "opt_tag",
						principalColumn: "TagId",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "opt_marketplaceproductreview",
				columns: table => new
				{
					MarketplaceProductReviewId = table.Column<int>(nullable: false)
						.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
					MarketplaceProductId = table.Column<int>(nullable: false),
					Text = table.Column<string>(nullable: true),
					AuthorName = table.Column<string>(nullable: true)
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
				name: "IX_opt_marketplaceproduct_MarketplaceId",
				table: "opt_marketplaceproduct",
				column: "MarketplaceId");

			migrationBuilder.CreateIndex(
				name: "IX_opt_marketplaceproduct_ProductId",
				table: "opt_marketplaceproduct",
				column: "ProductId");

			migrationBuilder.CreateIndex(
				name: "IX_opt_marketplaceproductreview_MarketplaceProductId",
				table: "opt_marketplaceproductreview",
				column: "MarketplaceProductId");

			migrationBuilder.CreateIndex(
				name: "IX_opt_productimage_ProductId",
				table: "opt_productimage",
				column: "ProductId");

			migrationBuilder.CreateIndex(
				name: "IX_opt_producttag_ProductId",
				table: "opt_producttag",
				column: "ProductId");

			migrationBuilder.CreateIndex(
				name: "IX_opt_producttag_TagId",
				table: "opt_producttag",
				column: "TagId");

			migrationBuilder.Sql(
				"INSERT INTO opt_tag (`TagId`, `TagName`, `OrderNumber`) VALUES(1, 'Вырезаем', 1); " +
				"INSERT INTO opt_tag (`TagId`, `TagName`, `OrderNumber`) VALUES(2, 'Рисуем', 2); " +
				"INSERT INTO opt_tag (`TagId`, `TagName`, `OrderNumber`) VALUES(3, 'Читаем', 3); " +
				"INSERT INTO opt_tag (`TagId`, `TagName`, `OrderNumber`) VALUES(4, 'Думаем', 4); "
				, true);

			migrationBuilder.Sql(
				"INSERT INTO opt_marketplace (`MarketplaceId`, `Name`) VALUES(1, 'WB'); " +
				"INSERT INTO opt_marketplace (`MarketplaceId`, `Name`) VALUES(2, 'ЯндексМаркет'); " +
				"INSERT INTO opt_marketplace (`MarketplaceId`, `Name`) VALUES(3, 'Ozon'); "
				, true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "opt_marketplaceproductreview");

			migrationBuilder.DropTable(
				name: "opt_productimage");

			migrationBuilder.DropTable(
				name: "opt_producttag");

			migrationBuilder.DropTable(
				name: "opt_marketplaceproduct");

			migrationBuilder.DropColumn(
				name: "SiteName",
				table: "opt_productseries");

			migrationBuilder.DropColumn(
				name: "Description",
				table: "opt_product");

			migrationBuilder.DropColumn(
				name: "Information",
				table: "opt_product");

			migrationBuilder.DropColumn(
				name: "LongDescription",
				table: "opt_product");

			migrationBuilder.DropColumn(
				name: "SiteDisplayOrder",
				table: "opt_product");

			migrationBuilder.DropColumn(
				name: "SiteEnabled",
				table: "opt_product");

			migrationBuilder.DropColumn(
				name: "SiteName",
				table: "opt_product");

			migrationBuilder.DropColumn(
				name: "VideoFileName",
				table: "opt_product");
		}
	}
}
