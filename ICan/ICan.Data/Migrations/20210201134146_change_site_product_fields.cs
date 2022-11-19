using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class change_site_product_fields : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "SiteDisplayOrder",
				table: "opt_product");

			migrationBuilder.DropColumn(
				name: "SiteEnabled",
				table: "opt_product");

			migrationBuilder.AddColumn<bool>(
				name: "IsInternal",
				table: "opt_sitefilter",
				nullable: false,
				defaultValue: false);

			migrationBuilder.Sql("UPDATE `opt_sitefilter` SET `IsInternal` = 1;", true);	

			migrationBuilder.Sql("INSERT INTO  `opt_sitefilter` (`SiteFilterId`, `Name`, `IsInternal`) VALUES (999, 'Главная страница', 1);", true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "IsInternal",
				table: "opt_sitefilter");

			migrationBuilder.AddColumn<int>(
				name: "SiteDisplayOrder",
				table: "opt_product",
				type: "int",
				nullable: true);

			migrationBuilder.AddColumn<bool>(
				name: "SiteEnabled",
				table: "opt_product",
				type: "tinyint(1)",
				nullable: false,
				defaultValue: false);
		}
	}
}
