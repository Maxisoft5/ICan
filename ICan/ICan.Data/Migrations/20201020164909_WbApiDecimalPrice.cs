using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class WbApiDecimalPrice : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<decimal>(
				name: "Price",
				table: "opt_wbWarehouse",
				nullable: false,
				oldClrType: typeof(int),
				oldType: "int");

			migrationBuilder.AlterColumn<decimal>(
				name: "Discount",
				table: "opt_wbWarehouse",
				nullable: false,
				oldClrType: typeof(int),
				oldType: "int");

			migrationBuilder.AlterColumn<decimal>(
				name: "TotalPrice",
				table: "opt_wbsale",
				nullable: false,
				oldClrType: typeof(int),
				oldType: "int");

			migrationBuilder.AlterColumn<decimal>(
				name: "DiscountPercent",
				table: "opt_wbsale",
				nullable: false,
				oldClrType: typeof(int),
				oldType: "int");

			migrationBuilder.AlterColumn<decimal>(
				name: "TotalPrice",
				table: "opt_wborder",
				nullable: false,
				oldClrType: typeof(int),
				oldType: "int");

			migrationBuilder.AlterColumn<decimal>(
				name: "PromoCodeDiscount",
				table: "opt_wborder",
				nullable: false,
				oldClrType: typeof(int),
				oldType: "int");

			migrationBuilder.AlterColumn<decimal>(
				name: "DiscountPercent",
				table: "opt_wborder",
				nullable: false,
				oldClrType: typeof(int),
				oldType: "int");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<int>(
				name: "Price",
				table: "opt_wbWarehouse",
				type: "int",
				nullable: false,
				oldClrType: typeof(decimal));

			migrationBuilder.AlterColumn<int>(
				name: "Discount",
				table: "opt_wbWarehouse",
				type: "int",
				nullable: false,
				oldClrType: typeof(decimal));

			migrationBuilder.AlterColumn<int>(
				name: "TotalPrice",
				table: "opt_wbsale",
				type: "int",
				nullable: false,
				oldClrType: typeof(decimal));

			migrationBuilder.AlterColumn<int>(
				name: "DiscountPercent",
				table: "opt_wbsale",
				type: "int",
				nullable: false,
				oldClrType: typeof(decimal));

			migrationBuilder.AlterColumn<int>(
				name: "TotalPrice",
				table: "opt_wborder",
				type: "int",
				nullable: false,
				oldClrType: typeof(decimal));

			migrationBuilder.AlterColumn<int>(
				name: "PromoCodeDiscount",
				table: "opt_wborder",
				type: "int",
				nullable: false,
				oldClrType: typeof(decimal));

			migrationBuilder.AlterColumn<int>(
				name: "DiscountPercent",
				table: "opt_wborder",
				type: "int",
				nullable: false,
				oldClrType: typeof(decimal));
		}
	}
}
