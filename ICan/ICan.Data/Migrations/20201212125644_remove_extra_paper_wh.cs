using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class remove_extra_paper_wh : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("DELETE FROM opt_warehousetype WHERE warehouseTypeId = 4;" +
				"update opt_warehousetype set Name='Склад готовой бумаги в Я Могу',  CounterpartyId = 11 where warehouseTypeId =3; " +
				"update opt_warehousetype set CounterpartyId = 1 where warehouseTypeId =5; " +
				"", true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{

		}
	}
}
