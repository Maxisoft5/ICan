using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class WbApiData : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "opt_wborder",
				columns: table => new
				{
					WbOrderID = table.Column<long>(type: "bigint", nullable: false)
						.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
					Number = table.Column<string>(nullable: true),
					Date = table.Column<DateTime>(nullable: false),
					LastChangeDate = table.Column<DateTime>(nullable: false),
					SupplierArticle = table.Column<string>(nullable: true),
					TechSize = table.Column<string>(nullable: true),
					Barcode = table.Column<string>(nullable: true),
					Quantity = table.Column<int>(nullable: false),
					TotalPrice = table.Column<int>(nullable: false),
					DiscountPercent = table.Column<int>(nullable: false),
					IsSupply = table.Column<bool>(nullable: false),
					IsRealization = table.Column<bool>(nullable: false),
					OrderId = table.Column<long>(nullable: false),
					PromoCodeDiscount = table.Column<int>(nullable: false),
					WarehouseName = table.Column<string>(nullable: true),
					CountryName = table.Column<string>(nullable: true),
					OblastOkrugName = table.Column<string>(nullable: true),
					RegionName = table.Column<string>(nullable: true),
					IncomeID = table.Column<int>(nullable: false),
					SaleID = table.Column<string>(nullable: true),
					Odid = table.Column<long>(nullable: false),
					Spp = table.Column<int>(nullable: false),
					ForPay = table.Column<double>(nullable: false),
					FinishedPrice = table.Column<double>(nullable: false),
					PriceWithDisc = table.Column<double>(nullable: false),
					NmId = table.Column<int>(nullable: false),
					Subject = table.Column<string>(nullable: true),
					Category = table.Column<string>(nullable: true),
					Brand = table.Column<string>(nullable: true),
					IsStorno = table.Column<int>(nullable: false),
					GNumber = table.Column<string>(nullable: true),
					ProductId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_opt_wborder", x => x.WbOrderID);
				});

			migrationBuilder.CreateTable(
				name: "opt_wbsale",
				columns: table => new
				{
					WbSaleID = table.Column<long>(type: "bigint", nullable: false)
						.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
					Number = table.Column<string>(nullable: true),
					Date = table.Column<DateTime>(nullable: false),
					LastChangeDate = table.Column<DateTime>(nullable: false),
					SupplierArticle = table.Column<string>(nullable: true),
					TechSize = table.Column<string>(nullable: true),
					Barcode = table.Column<string>(nullable: true),
					Quantity = table.Column<int>(nullable: false),
					TotalPrice = table.Column<int>(nullable: false),
					DiscountPercent = table.Column<int>(nullable: false),
					IsSupply = table.Column<bool>(nullable: false),
					IsRealization = table.Column<bool>(nullable: false),
					OrderId = table.Column<long>(nullable: false),
					PromoCodeDiscount = table.Column<int>(nullable: false),
					WarehouseName = table.Column<string>(nullable: true),
					CountryName = table.Column<string>(nullable: true),
					OblastOkrugName = table.Column<string>(nullable: true),
					RegionName = table.Column<string>(nullable: true),
					IncomeID = table.Column<int>(nullable: false),
					SaleID = table.Column<string>(nullable: true),
					Odid = table.Column<long>(nullable: false),
					Spp = table.Column<int>(nullable: false),
					ForPay = table.Column<double>(nullable: false),
					FinishedPrice = table.Column<double>(nullable: false),
					PriceWithDisc = table.Column<double>(nullable: false),
					NmId = table.Column<int>(nullable: false),
					Subject = table.Column<string>(nullable: true),
					Category = table.Column<string>(nullable: true),
					Brand = table.Column<string>(nullable: true),
					IsStorno = table.Column<int>(nullable: false),
					GNumber = table.Column<string>(nullable: true),
					ProductId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_opt_wbsale", x => x.WbSaleID);
				});

			migrationBuilder.CreateTable(
				name: "opt_wbWarehouse",
				columns: table => new
				{
					WbWarehouseID = table.Column<long>(type: "bigint", nullable: false)
						.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
					LastChangeDate = table.Column<DateTime>(nullable: false),
					SupplierArticle = table.Column<string>(nullable: true),
					TechSize = table.Column<string>(nullable: true),
					Barcode = table.Column<string>(nullable: true),
					Quantity = table.Column<int>(nullable: false),
					IsSupply = table.Column<bool>(nullable: false),
					IsRealization = table.Column<bool>(nullable: false),
					QuantityFull = table.Column<int>(nullable: false),
					QuantityNotInOrders = table.Column<int>(nullable: false),
					WarehouseName = table.Column<string>(nullable: true),
					InWayToClient = table.Column<int>(nullable: false),
					InWayFromClient = table.Column<int>(nullable: false),
					NmId = table.Column<int>(nullable: false),
					Subject = table.Column<string>(nullable: true),
					Category = table.Column<string>(nullable: true),
					DaysOnSite = table.Column<int>(nullable: false),
					Brand = table.Column<string>(nullable: true),
					SCCode = table.Column<string>(nullable: true),
					Price = table.Column<int>(nullable: false),
					Discount = table.Column<int>(nullable: false),
					ProductId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_opt_wbWarehouse", x => x.WbWarehouseID);
				});
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "opt_wborder");

			migrationBuilder.DropTable(
				name: "opt_wbsale");

			migrationBuilder.DropTable(
				name: "opt_wbWarehouse");
		}
	}
}
