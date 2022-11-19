using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_foreign_countries : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<int>(
				name: "CountryId",
				table: "opt_product",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "RegionalName",
				table: "opt_product",
				nullable: true);

			migrationBuilder.CreateTable(
				name: "opt_country",
				columns: table => new
				{
					CountryId = table.Column<int>(nullable: false)
						.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
					Name = table.Column<string>(nullable: true),
					Prefix = table.Column<string>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_opt_country", x => x.CountryId);
				});

			migrationBuilder.CreateIndex(
				name: "IX_opt_product_CountryId",
				table: "opt_product",
				column: "CountryId");

			migrationBuilder.AddForeignKey(
				name: "Opt_Product_CountryId",
				table: "opt_product",
				column: "CountryId",
				principalTable: "opt_country",
				principalColumn: "CountryId",
				onDelete: ReferentialAction.Restrict);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "Opt_Product_CountryId",
				table: "opt_product");

			migrationBuilder.DropTable(
				name: "opt_country");

			migrationBuilder.DropIndex(
				name: "IX_opt_product_CountryId",
				table: "opt_product");

			migrationBuilder.DropColumn(
				name: "CountryId",
				table: "opt_product");

			migrationBuilder.DropColumn(
				name: "RegionalName",
				table: "opt_product");
		}
	}
}
