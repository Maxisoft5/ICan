using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_fk_paperorderincoming : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropPrimaryKey(
				name: "PK_opt_paperorderincomings",
				table: "opt_paperorderincomings");

			migrationBuilder.RenameTable(
				name: "opt_paperorderincomings",
				newName: "opt_paperorderincoming");

			migrationBuilder.RenameIndex(
				name: "IX_opt_paperorderincomings_PaperOrderId",
				table: "opt_paperorderincoming",
				newName: "IX_opt_paperorderincoming_PaperOrderId");

			migrationBuilder.AddPrimaryKey(
				name: "PK_opt_paperorderincoming",
				table: "opt_paperorderincoming",
				column: "PaperOrderIncomingId");

			migrationBuilder.CreateIndex(
				name: "IX_opt_paperorderincoming_WarehouseTypeId",
				table: "opt_paperorderincoming",
				column: "WarehouseTypeId");

			migrationBuilder.AddForeignKey(
				name: "Opt_PaperOrderIncomig_WarehouseTypeID",
				table: "opt_paperorderincoming",
				column: "WarehouseTypeId",
				principalTable: "opt_warehousetype",
				principalColumn: "WarehouseTypeId",
				onDelete: ReferentialAction.Restrict);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "Opt_PaperOrderIncomig_WarehouseTypeID",
				table: "opt_paperorderincoming");

			migrationBuilder.DropPrimaryKey(
				name: "PK_opt_paperorderincoming",
				table: "opt_paperorderincoming");

			migrationBuilder.DropIndex(
				name: "IX_opt_paperorderincoming_WarehouseTypeId",
				table: "opt_paperorderincoming");

			migrationBuilder.RenameTable(
				name: "opt_paperorderincoming",
				newName: "opt_paperorderincomings");

			migrationBuilder.RenameIndex(
				name: "IX_opt_paperorderincoming_PaperOrderId",
				table: "opt_paperorderincomings",
				newName: "IX_opt_paperorderincomings_PaperOrderId");

			migrationBuilder.AddPrimaryKey(
				name: "PK_opt_paperorderincomings",
				table: "opt_paperorderincomings",
				column: "PaperOrderIncomingId");
		}
	}
}
