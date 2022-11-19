using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_notch_stckers_data : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateIndex(
				name: "IX_opt_movepaper_ReceiverWarehouseId",
				unique : false,
				table: "opt_movepaper",
				column: "ReceiverWarehouseId");

			migrationBuilder.CreateIndex(
				name: "IX_opt_movepaper_SenderWarehouseId",
				table: "opt_movepaper",
				column: "SenderWarehouseId");

			migrationBuilder.AddForeignKey(
				name: "FK_opt_movepaper_opt_warehousetype_ReceiverWarehouseId",
				table: "opt_movepaper",
				column: "ReceiverWarehouseId",
				principalTable: "opt_warehousetype",
				principalColumn: "WarehouseTypeId",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_opt_movepaper_opt_warehousetype_SenderWarehouseId",
				table: "opt_movepaper",
				column: "SenderWarehouseId",
				principalTable: "opt_warehousetype",
				principalColumn: "WarehouseTypeId",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.Sql(@"INSERT INTO opt_notchordersticker                    (NotchOrderID, SemiproductID, IsAssembled)
                            SELECT m.NotchOrderID, t.SemiproductID, 0 FROM  opt_notchorderitem  m
                            JOIN opt_printordersemiproduct t ON m.PrintOrderId = t.PrintOrderId ");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_opt_movepaper_opt_warehousetype_ReceiverWarehouseId",
				table: "opt_movepaper");

			migrationBuilder.DropForeignKey(
				name: "FK_opt_movepaper_opt_warehousetype_SenderWarehouseId",
				table: "opt_movepaper");

			migrationBuilder.DropIndex(
				name: "IX_opt_movepaper_ReceiverWarehouseId",
				table: "opt_movepaper");

			migrationBuilder.DropIndex(
				name: "IX_opt_movepaper_SenderWarehouseId",
				table: "opt_movepaper");
		}
	}
}
