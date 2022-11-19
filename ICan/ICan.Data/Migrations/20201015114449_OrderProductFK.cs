using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class OrderProductFK : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "Opt_OrderProductPrice",
				table: "opt_orderproduct");

			migrationBuilder.AddForeignKey(
				name: "Opt_OrderPayment_OrderId",
				table: "opt_orderpayment",
				column: "OrderID",
				principalTable: "opt_order",
				principalColumn: "OrderID",
				onDelete: ReferentialAction.Restrict);

			migrationBuilder.AddForeignKey(
				name: "Opt_OrderProduct_OrderId",
				table: "opt_orderproduct",
				column: "OrderID",
				principalTable: "opt_order",
				principalColumn: "OrderID",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "Opt_OrderProduct_ProductId",
				table: "opt_orderproduct",
				column: "ProductID",
				principalTable: "opt_product",
				principalColumn: "ProductID",
				onDelete: ReferentialAction.Restrict);

			migrationBuilder.AddForeignKey(
				name: "Opt_Orderproduct_ProductPriceId",
				table: "opt_orderproduct",
				column: "ProductPriceId",
				principalTable: "opt_productprice",
				principalColumn: "ProductPriceId",
				onDelete: ReferentialAction.Restrict);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "Opt_OrderPayment_OrderId",
				table: "opt_orderpayment");

			migrationBuilder.DropForeignKey(
				name: "Opt_OrderProduct_OrderId",
				table: "opt_orderproduct");

			migrationBuilder.DropForeignKey(
				name: "Opt_OrderProduct_ProductId",
				table: "opt_orderproduct");

			migrationBuilder.DropForeignKey(
				name: "Opt_Orderproduct_ProductPriceId",
				table: "opt_orderproduct");

			migrationBuilder.AddForeignKey(
				name: "Opt_OrderPayment",
				table: "opt_orderpayment",
				column: "OrderID",
				principalTable: "opt_order",
				principalColumn: "OrderID",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "Opt_OrderProduct_Order",
				table: "opt_orderproduct",
				column: "OrderID",
				principalTable: "opt_order",
				principalColumn: "OrderID",
				onDelete: ReferentialAction.Restrict);

			migrationBuilder.AddForeignKey(
				name: "Opt_OrderProduct_Product",
				table: "opt_orderproduct",
				column: "ProductID",
				principalTable: "opt_product",
				principalColumn: "ProductID",
				onDelete: ReferentialAction.Restrict);

			migrationBuilder.AddForeignKey(
				name: "Opt_OrderProductPrice",
				table: "opt_orderproduct",
				column: "ProductPriceId",
				principalTable: "opt_productprice",
				principalColumn: "ProductPriceId",
				onDelete: ReferentialAction.Restrict);
		}
	}
}
