using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_warehouse_for_gluepads : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("INSERT INTO opt_warehousetype (`WarehouseTypeId`, `Name`, `WarehouseObjectType`, `ReadyToUse`) VALUES(8, 'Склад клеевых подушек', 2, 1)");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("DELETE FROM opt_warehousetype where WarehouseTypeId = 8");
		}
	}
}
