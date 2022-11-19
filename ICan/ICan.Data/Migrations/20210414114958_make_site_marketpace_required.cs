using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class make_site_marketpace_required : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_opt_marketplace_opt_site_SiteId",
				table: "opt_marketplace");

			migrationBuilder.AlterColumn<int>(
				name: "SiteId",
				table: "opt_marketplace",
				nullable: false,
				oldClrType: typeof(int),
				oldType: "int",
				oldNullable: true);

			migrationBuilder.AddForeignKey(
				name: "FK_opt_marketplace_opt_site_SiteId",
				table: "opt_marketplace",
				column: "SiteId",
				principalTable: "opt_site",
				principalColumn: "SiteId",
				onDelete: ReferentialAction.Cascade);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_opt_marketplace_opt_site_SiteId",
				table: "opt_marketplace");

			migrationBuilder.AlterColumn<int>(
				name: "SiteId",
				table: "opt_marketplace",
				type: "int",
				nullable: true,
				oldClrType: typeof(int));

			migrationBuilder.AddForeignKey(
				name: "FK_opt_marketplace_opt_site_SiteId",
				table: "opt_marketplace",
				column: "SiteId",
				principalTable: "opt_site",
				principalColumn: "SiteId",
				onDelete: ReferentialAction.Restrict);
		}
	}
}
