using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class WbOrderChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinishedPrice",
                table: "opt_wborder");

            migrationBuilder.DropColumn(
                name: "ForPay",
                table: "opt_wborder");

            migrationBuilder.DropColumn(
                name: "GNumber",
                table: "opt_wborder");

            migrationBuilder.DropColumn(
                name: "IsRealization",
                table: "opt_wborder");

            migrationBuilder.DropColumn(
                name: "IsStorno",
                table: "opt_wborder");

            migrationBuilder.DropColumn(
                name: "IsSupply",
                table: "opt_wborder");

            migrationBuilder.DropColumn(
                name: "OblastOkrugName",
                table: "opt_wborder");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "opt_wborder");

            migrationBuilder.DropColumn(
                name: "PriceWithDisc",
                table: "opt_wborder");

            migrationBuilder.DropColumn(
                name: "PromoCodeDiscount",
                table: "opt_wborder");

            migrationBuilder.DropColumn(
                name: "RegionName",
                table: "opt_wborder");

            migrationBuilder.DropColumn(
                name: "SaleID",
                table: "opt_wborder");

            migrationBuilder.DropColumn(
                name: "Spp",
                table: "opt_wborder");

            //migrationBuilder.RenameColumn(
            //    name: "IncomeID",
            //    table: "opt_wborder",
            //    newName: "IncomeId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelDate",
                table: "opt_wborder",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "IsCancel",
                table: "opt_wborder",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Oblast",
                table: "opt_wborder",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelDate",
                table: "opt_wborder");

            migrationBuilder.DropColumn(
                name: "IsCancel",
                table: "opt_wborder");

            migrationBuilder.DropColumn(
                name: "Oblast",
                table: "opt_wborder");

            //migrationBuilder.RenameColumn(
            //    name: "IncomeId",
            //    table: "opt_wborder",
            //    newName: "IncomeID");

            migrationBuilder.AddColumn<double>(
                name: "FinishedPrice",
                table: "opt_wborder",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ForPay",
                table: "opt_wborder",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "GNumber",
                table: "opt_wborder",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRealization",
                table: "opt_wborder",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "IsStorno",
                table: "opt_wborder",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsSupply",
                table: "opt_wborder",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OblastOkrugName",
                table: "opt_wborder",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "OrderId",
                table: "opt_wborder",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<double>(
                name: "PriceWithDisc",
                table: "opt_wborder",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<decimal>(
                name: "PromoCodeDiscount",
                table: "opt_wborder",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RegionName",
                table: "opt_wborder",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaleID",
                table: "opt_wborder",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Spp",
                table: "opt_wborder",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
