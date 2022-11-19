using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_springs_wh : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("INSERT INTO opt_warehousetype (`WarehouseTypeId`, `Name`, `WarehouseObjectType`, `ReadyToUse`) VALUES(9, 'Пружины',5 , 1);");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("delete from opt_warehousetype where `WarehouseTypeId` = 9 ");
		}
	}
}
