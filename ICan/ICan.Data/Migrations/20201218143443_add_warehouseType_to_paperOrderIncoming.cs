using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_warehouseType_to_paperOrderIncoming : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<int>(
				name: "WarehouseTypeId",
				table: "opt_paperorderincomings",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.Sql("update opt_paperorderincomings as incomings " +
								 "set WarehouseTypeId = (select WarehouseTypeId " +
														"from opt_warehousetype wtype " +
														"join opt_counterparty party on wtype.CounterpartyId = party.CounterpartyId " +
														"join opt_paperorder paper on paper.RecieverCounterpartyId = party.CounterpartyId " +
														"where paper.PaperOrderId = incomings.PaperOrderId)	 													where PaperOrderIncomingId > 0;", true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "WarehouseTypeId",
				table: "opt_paperorderincomings");
		}
	}
}
