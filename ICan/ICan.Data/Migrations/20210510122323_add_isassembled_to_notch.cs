using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_isassembled_to_notch : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{

			migrationBuilder.Sql("ALTER TABLE `opt_wbwarehouse` RENAME TO `opt_wbWarehouse` ");

			migrationBuilder.Sql("ALTER TABLE `aspnetuserlogins` RENAME TO `AspNetUserLogins` ");

			migrationBuilder.AddColumn<bool>(
				name: "IsAssembled",
				table: "opt_notchorder",
				type: "tinyint(1)",
				nullable: false,
				defaultValue: false);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "IsAssembled",
				table: "opt_notchorder");
		}
	}
}
