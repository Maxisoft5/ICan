using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class semiproduct_as_notch_order_item : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("truncate table opt_notchorderincomingitem", true);

			migrationBuilder.DropForeignKey(
				name: "Opt_NotchOredIncomingItem_NotchOrderItemId",
				table: "opt_notchorderincomingitem");

			migrationBuilder.DropIndex(
				name: "Opt_NotchOredIncomingItem_NotchOrderItemID",
				table: "opt_notchorderincomingitem");

			migrationBuilder.DropColumn(
				name: "NotchOrderItemId",
				table: "opt_notchorderincomingitem");

			migrationBuilder.AddColumn<int>(
				name: "SemiproductId",
				table: "opt_notchorderincomingitem",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.CreateIndex(
				name: "IX_opt_notchorderincomingitem_SemiproductId",
				table: "opt_notchorderincomingitem",
				column: "SemiproductId");

			migrationBuilder.AddForeignKey(
				name: "Opt_NotchOredIncomingItem_SemiproductId",
				table: "opt_notchorderincomingitem",
				column: "SemiproductId",
				principalTable: "opt_semiproduct",
				principalColumn: "SemiproductID",
				onDelete: ReferentialAction.Restrict);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "Opt_NotchOredIncomingItem_SemiproductId",
				table: "opt_notchorderincomingitem");

			migrationBuilder.DropIndex(
				name: "IX_opt_notchorderincomingitem_SemiproductId",
				table: "opt_notchorderincomingitem");

			migrationBuilder.DropColumn(
				name: "SemiproductId",
				table: "opt_notchorderincomingitem");

			migrationBuilder.AddColumn<int>(
				name: "NotchOrderItemId",
				table: "opt_notchorderincomingitem",
				type: "int",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.CreateIndex(
				name: "Opt_NotchOredIncomingItem_NotchOrderItemID",
				table: "opt_notchorderincomingitem",
				column: "NotchOrderItemId");

			migrationBuilder.AddForeignKey(
				name: "Opt_NotchOredIncomingItem_NotchOrderItemId",
				table: "opt_notchorderincomingitem",
				column: "NotchOrderItemId",
				principalTable: "opt_notchorderitem",
				principalColumn: "NotchOrderItemId",
				onDelete: ReferentialAction.Restrict);
		}
	}
}
