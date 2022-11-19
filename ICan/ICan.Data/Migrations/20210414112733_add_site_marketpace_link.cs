using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_site_marketpace_link : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameColumn(
				name: "SaleID",
				table: "opt_wbsale",
				newName: "SaleId");

			migrationBuilder.RenameColumn(
				name: "IncomeID",
				table: "opt_wbsale",
				newName: "IncomeId");

			migrationBuilder.AddColumn<int>(
				name: "IncomeId",
				table: "opt_wbWarehouse",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "Number",
				table: "opt_wbWarehouse",
				nullable: true);

			migrationBuilder.AddColumn<long>(
				name: "Odid",
				table: "opt_wbWarehouse",
				nullable: false,
				defaultValue: 0L);

			migrationBuilder.AlterColumn<int>(
				name: "IncomeId",
				table: "opt_wbsale",
				nullable: true,
				oldClrType: typeof(int),
				oldType: "int");

			migrationBuilder.AddColumn<int>(
				name: "SiteId",
				table: "opt_marketplace",
				nullable: true);

			migrationBuilder.CreateIndex(
				name: "IX_opt_marketplace_SiteId",
				table: "opt_marketplace",
				column: "SiteId");

			migrationBuilder.AddForeignKey(
				name: "FK_opt_marketplace_opt_site_SiteId",
				table: "opt_marketplace",
				column: "SiteId",
				principalTable: "opt_site",
				principalColumn: "SiteId",
				onDelete: ReferentialAction.Restrict);

			migrationBuilder.Sql("update opt_marketplace set `SiteId`= 1 where `MarketplaceId` = 1; ", true);
			migrationBuilder.Sql("update opt_marketplace set `SiteId`= 1 where `MarketplaceId` = 2; ", true);
			migrationBuilder.Sql("update opt_marketplace set `SiteId`= 1 where `MarketplaceId` = 3; ", true);
			migrationBuilder.Sql("update opt_marketplace set `SiteId`= 2 where `MarketplaceId` = 4; ", true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_opt_marketplace_opt_site_SiteId",
				table: "opt_marketplace");

			migrationBuilder.DropIndex(
				name: "IX_opt_marketplace_SiteId",
				table: "opt_marketplace");

			migrationBuilder.DropColumn(
				name: "IncomeId",
				table: "opt_wbWarehouse");

			migrationBuilder.DropColumn(
				name: "Number",
				table: "opt_wbWarehouse");

			migrationBuilder.DropColumn(
				name: "Odid",
				table: "opt_wbWarehouse");

			migrationBuilder.DropColumn(
				name: "SiteId",
				table: "opt_marketplace");

			migrationBuilder.RenameColumn(
				name: "SaleId",
				table: "opt_wbsale",
				newName: "SaleID");

			migrationBuilder.RenameColumn(
				name: "IncomeId",
				table: "opt_wbsale",
				newName: "IncomeID");

			migrationBuilder.AlterColumn<int>(
				name: "IncomeID",
				table: "opt_wbsale",
				type: "int",
				nullable: false,
				oldClrType: typeof(int),
				oldNullable: true);
		}
	}
}
