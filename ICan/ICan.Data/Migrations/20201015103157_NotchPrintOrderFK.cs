using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class NotchPrintOrderFK : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "Opt_OptNotchOrderItem_PrintOrderId",
				table: "opt_notchorderitem");

			migrationBuilder.AddForeignKey(
				name: "Opt_NotchOrderItem_NotchOrderId",
				table: "opt_notchorderitem",
				column: "NotchOrderId",
				principalTable: "opt_notchorder",
				principalColumn: "NotchOrderId",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "Opt_NotchOrderItem_PrintOrderId",
				table: "opt_notchorderitem",
				column: "PrintOrderId",
				principalTable: "opt_printorder",
				principalColumn: "PrintOrderID",
				onDelete: ReferentialAction.Restrict);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "Opt_NotchOrderItem_NotchOrderId",
				table: "opt_notchorderitem");

			migrationBuilder.DropForeignKey(
				name: "Opt_NotchOrderItem_PrintOrderId",
				table: "opt_notchorderitem");

			migrationBuilder.AddForeignKey(
				name: "Opt_OptNotchOrderItem_PrintOrderId",
				table: "opt_notchorderitem",
				column: "PrintOrderId",
				principalTable: "opt_printorder",
				principalColumn: "PrintOrderID",
				onDelete: ReferentialAction.Restrict);
		}
	}
}
