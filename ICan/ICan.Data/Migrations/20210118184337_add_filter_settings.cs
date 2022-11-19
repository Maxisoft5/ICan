using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_filter_settings : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "opt_sitefilter",
				columns: table => new
				{
					SiteFilterId = table.Column<int>(nullable: false)
						.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
					Name = table.Column<string>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_opt_sitefilter", x => x.SiteFilterId);
				});

			migrationBuilder.CreateTable(
				name: "opt_sitefilterproduct",
				columns: table => new
				{
					SiteFilterProductId = table.Column<int>(nullable: false)
						.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
					SiteFilterId = table.Column<int>(nullable: false),
					ProductId = table.Column<int>(nullable: false),
					Order = table.Column<int>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_opt_sitefilterproduct", x => x.SiteFilterProductId);
					table.ForeignKey(
						name: "Opt_SiteFilterProduct_ProductId",
						column: x => x.ProductId,
						principalTable: "opt_product",
						principalColumn: "ProductID",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "Opt_SiteFilterProduct_SiteFilterId",
						column: x => x.SiteFilterId,
						principalTable: "opt_sitefilter",
						principalColumn: "SiteFilterId",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_opt_sitefilterproduct_ProductId",
				table: "opt_sitefilterproduct",
				column: "ProductId");

			migrationBuilder.CreateIndex(
				name: "IX_opt_sitefilterproduct_SiteFilterId",
				table: "opt_sitefilterproduct",
				column: "SiteFilterId");

			migrationBuilder.Sql("INSERT INTO  `opt_sitefilter` (`SiteFilterId`, `Name`) VALUES (1, '2-3');", true);
			migrationBuilder.Sql("INSERT INTO  `opt_sitefilter` (`SiteFilterId`, `Name`) VALUES (2, '3-4');", true);
			migrationBuilder.Sql("INSERT INTO  `opt_sitefilter` (`SiteFilterId`, `Name`) VALUES (3, '4-5');", true);
			migrationBuilder.Sql("INSERT INTO  `opt_sitefilter` (`SiteFilterId`, `Name`) VALUES (4, '5+');", true);
			migrationBuilder.Sql("INSERT INTO  `opt_sitefilter` (`SiteFilterId`, `Name`) VALUES (5, 'Подарок');", true);


			migrationBuilder.Sql("INSERT INTO `opt_sitefilterproduct` (`ProductId`, `SiteFilterId`)" +
				" select productId, 1 from opt_product where ProductSeriesId = 2;", true);

			migrationBuilder.Sql("INSERT INTO `opt_sitefilterproduct` (`ProductId`, `SiteFilterId`)" +
				" select productId, 2 from opt_product where ProductSeriesId = 3;", true);

			migrationBuilder.Sql("INSERT INTO `opt_sitefilterproduct` (`ProductId`, `SiteFilterId`)" +
				" select productId, 3 from opt_product where ProductSeriesId = 4;", true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "opt_sitefilterproduct");

			migrationBuilder.DropTable(
				name: "opt_sitefilter");
		}
	}
}
