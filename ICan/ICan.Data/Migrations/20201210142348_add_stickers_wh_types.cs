using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_stickers_wh_types : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("INSERT INTO opt_warehousetype (`WarehouseTypeId`, `Name`, `WarehouseObjectType`, `ReadyToUse`) VALUES(4, 'Склад готовой бумаги в Я Могу', 3, 1); " +
				"INSERT INTO opt_warehousetype (`WarehouseTypeId`, `Name`, `WarehouseObjectType`, `ReadyToUse`) VALUES(5, 'Склад готовой бумаги в Зетапринт', 3, 1); " +
				"INSERT INTO opt_warehousetype (`WarehouseTypeId`, `Name`, `WarehouseObjectType`, `ReadyToUse`) VALUES(6, 'Не надсечённые наклейки на складе в Я Могу', 2, 0); " +
				"INSERT INTO opt_warehousetype (`WarehouseTypeId`, `Name`, `WarehouseObjectType`, `ReadyToUse`) VALUES(7, 'Наклейки на надсечке в Аксае', 2, 0); ", true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("DELETE FROM opt_warehousetype where WarehouseTypeId > 3", true);
		}
	}
}
