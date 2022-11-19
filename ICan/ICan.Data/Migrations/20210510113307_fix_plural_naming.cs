using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class fix_plural_naming : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropPrimaryKey(
				name: "PK_opt_springs",
				table: "opt_springs");

			migrationBuilder.DropPrimaryKey(
				name: "PK_opt_gluepadincomings",
				table: "opt_gluepadincomings");

			migrationBuilder.RenameTable(
				name: "opt_springs",
				newName: "opt_spring");

			migrationBuilder.RenameTable(
				name: "opt_gluepadincomings",
				newName: "opt_gluepadincoming");

			migrationBuilder.RenameIndex(
				name: "IX_opt_springs_NumberOfTurnsId",
				table: "opt_spring",
				newName: "IX_opt_spring_NumberOfTurnsId");

			migrationBuilder.AddPrimaryKey(
				name: "PK_opt_spring",
				table: "opt_spring",
				column: "SpringId");

			migrationBuilder.AddPrimaryKey(
				name: "PK_opt_gluepadincoming",
				table: "opt_gluepadincoming",
				column: "Id");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropPrimaryKey(
				name: "PK_opt_spring",
				table: "opt_spring");

			migrationBuilder.DropPrimaryKey(
				name: "PK_opt_gluepadincoming",
				table: "opt_gluepadincoming");

			migrationBuilder.RenameTable(
				name: "opt_spring",
				newName: "opt_springs");

			migrationBuilder.RenameTable(
				name: "opt_gluepadincoming",
				newName: "opt_gluepadincomings");

			migrationBuilder.RenameIndex(
				name: "IX_opt_spring_NumberOfTurnsId",
				table: "opt_springs",
				newName: "IX_opt_springs_NumberOfTurnsId");

			migrationBuilder.AddPrimaryKey(
				name: "PK_opt_springs",
				table: "opt_springs",
				column: "SpringId");

			migrationBuilder.AddPrimaryKey(
				name: "PK_opt_gluepadincomings",
				table: "opt_gluepadincomings",
				column: "Id");
		}
	}
}
