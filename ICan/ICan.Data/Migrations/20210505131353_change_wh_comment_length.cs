using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class change_wh_comment_length : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "Comment",
				table: "opt_warehouse",
				type: "VARCHAR(200)",
				nullable: true,
				oldClrType: typeof(string),
				oldType: "longtext CHARACTER SET utf8mb4",
				oldNullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "Comment",
				table: "opt_warehouse",
				type: "longtext CHARACTER SET utf8mb4",
				nullable: true,
				oldClrType: typeof(string),
				oldType: "VARCHAR(200)",
				oldNullable: true);
		}
	}
}
