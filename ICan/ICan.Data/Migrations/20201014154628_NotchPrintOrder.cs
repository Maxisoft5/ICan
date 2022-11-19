using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class NotchPrintOrder : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "Opt_NotchOrderIncoming_NotchOrder_NotchOrderId",
				table: "opt_notchorderincoming");

			migrationBuilder.DropForeignKey(
				name: "Opt_NotchOredIncoming_NotchOrderIncomingId",
				table: "opt_notchorderincomingitem");

			migrationBuilder.DropForeignKey(
				name: "Opt_NotchOrderItem_NotchOrderItemId",
				table: "opt_notchorderincomingitem");

			migrationBuilder.DropForeignKey(
				name: "Opt_NotchOrder_NotchOrderID",
				table: "opt_notchorderitem");

			migrationBuilder.DropForeignKey(
				name: "Opt_NotchOrder_PrintOrderSemiproductID",
				table: "opt_notchorderitem");

			migrationBuilder.DropColumn(
				name: "PrintOrderSemiproductId",
				table: "opt_notchorderitem");

			migrationBuilder.AddColumn<int>(
				name: "PrintOrderId",
				table: "opt_notchorderitem",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.CreateIndex(
				name: "IX_opt_notchorderitem_PrintOrderId",
				table: "opt_notchorderitem",
				column: "PrintOrderId",
				unique: true);

			migrationBuilder.AddForeignKey(
				name: "Opt_NotchOrderIncoming_NotchOrderId",
				table: "opt_notchorderincoming",
				column: "NotchOrderId",
				principalTable: "opt_notchorder",
				principalColumn: "NotchOrderId",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "Opt_NotchOredIncomingItem_NotchOrderIncomingId",
				table: "opt_notchorderincomingitem",
				column: "NotchOrderIncomingId",
				principalTable: "opt_notchorderincoming",
				principalColumn: "NotchOrderIncomingId",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "Opt_NotchOredIncomingItem_NotchOrderItemId",
				table: "opt_notchorderincomingitem",
				column: "NotchOrderItemId",
				principalTable: "opt_notchorderitem",
				principalColumn: "NotchOrderItemId",
				onDelete: ReferentialAction.Restrict);

			migrationBuilder.AddForeignKey(
				name: "Opt_OptNotchOrderItem_PrintOrderId",
				table: "opt_notchorderitem",
				column: "PrintOrderId",
				principalTable: "opt_printorder",
				principalColumn: "PrintOrderID",
				onDelete: ReferentialAction.Restrict);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "Opt_NotchOrderIncoming_NotchOrderId",
				table: "opt_notchorderincoming");

			migrationBuilder.DropForeignKey(
				name: "Opt_NotchOredIncomingItem_NotchOrderIncomingId",
				table: "opt_notchorderincomingitem");

			migrationBuilder.DropForeignKey(
				name: "Opt_NotchOredIncomingItem_NotchOrderItemId",
				table: "opt_notchorderincomingitem");

			migrationBuilder.DropForeignKey(
				name: "Opt_OptNotchOrderItem_PrintOrderId",
				table: "opt_notchorderitem");

			migrationBuilder.DropIndex(
				name: "IX_opt_notchorderitem_PrintOrderId",
				table: "opt_notchorderitem");

			migrationBuilder.DropColumn(
				name: "PrintOrderId",
				table: "opt_notchorderitem");

			migrationBuilder.AddColumn<int>(
				name: "PrintOrderSemiproductId",
				table: "opt_notchorderitem",
				type: "int",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.CreateIndex(
				name: "IX_opt_notchorderitem_PrintOrderSemiproductId",
				table: "opt_notchorderitem",
				column: "PrintOrderSemiproductId");

			migrationBuilder.AddForeignKey(
				name: "Opt_NotchOrderIncoming_NotchOrder_NotchOrderId",
				table: "opt_notchorderincoming",
				column: "NotchOrderId",
				principalTable: "opt_notchorder",
				principalColumn: "NotchOrderId",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "Opt_NotchOredIncoming_NotchOrderIncomingId",
				table: "opt_notchorderincomingitem",
				column: "NotchOrderIncomingId",
				principalTable: "opt_notchorderincoming",
				principalColumn: "NotchOrderIncomingId",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "Opt_NotchOrderItem_NotchOrderItemId",
				table: "opt_notchorderincomingitem",
				column: "NotchOrderItemId",
				principalTable: "opt_notchorderitem",
				principalColumn: "NotchOrderItemId",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_opt_notchorderitem_opt_printordersemiproduct_PrintOrderSemip~",
				table: "opt_notchorderitem",
				column: "PrintOrderSemiproductId",
				principalTable: "opt_printordersemiproduct",
				principalColumn: "PrintOrderSemiproductID",
				onDelete: ReferentialAction.Cascade);
		}
	}
}
