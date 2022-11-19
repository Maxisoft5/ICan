using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_fields_to_notch : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<decimal>(
				name: "OrderSum",
				table: "opt_notchorder",
				type: "decimal(65,30)",
				nullable: true);

			migrationBuilder.AddColumn<DateTime>(
				name: "ShipmentDate",
				table: "opt_notchorder",
				type: "datetime(6)",
				nullable: true);

			migrationBuilder.AddColumn<decimal>(
				name: "ShipmentSum",
				table: "opt_notchorder",
				type: "decimal(65,30)",
				nullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "OrderSum",
				table: "opt_notchorder");

			migrationBuilder.DropColumn(
				name: "ShipmentDate",
				table: "opt_notchorder");

			migrationBuilder.DropColumn(
				name: "ShipmentSum",
				table: "opt_notchorder");
		}
	}
}
