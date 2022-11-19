using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class Add_wind_to_assembly : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<int>(
				name: "AssemblyType",
				table: "opt_assembly",
				nullable: false,
				defaultValue: 0);
			migrationBuilder.Sql("update opt_assembly set AssemblyType = 1 where AssemblyId > 0 AND AssemblyType = 0;", true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "AssemblyType",
				table: "opt_assembly");
		}
	}
}
