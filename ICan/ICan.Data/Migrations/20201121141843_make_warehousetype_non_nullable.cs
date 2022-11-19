using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class make_warehousetype_non_nullable : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("UPDATE opt_warehouse set WarehouseTypeId = 1 where WarehouseId > 0;", true);

			migrationBuilder.Sql("UPDATE opt_warehousejournal set WarehouseTypeId = 1 where ObjectTypeId = 1 and WarehousejournalID > 0;" +
				"UPDATE opt_warehousejournal set WarehouseTypeId = 2 where ObjectTypeId = 2 and WarehousejournalID > 0;", true);

			migrationBuilder.AlterColumn<int>(
				name: "WarehouseTypeId",
				table: "opt_warehousejournal",
				nullable: false,
				oldClrType: typeof(int),
				oldType: "int",
				oldNullable: true);

			migrationBuilder.AlterColumn<int>(
				name: "WarehouseTypeId",
				table: "opt_warehouse",
				nullable: false,
				oldClrType: typeof(int),
				oldType: "int",
				oldNullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<int>(
				name: "WarehouseTypeId",
				table: "opt_warehousejournal",
				type: "int",
				nullable: true,
				oldClrType: typeof(int));

			migrationBuilder.AlterColumn<int>(
				name: "WarehouseTypeId",
				table: "opt_warehouse",
				type: "int",
				nullable: true,
				oldClrType: typeof(int));
		}
	}
}
