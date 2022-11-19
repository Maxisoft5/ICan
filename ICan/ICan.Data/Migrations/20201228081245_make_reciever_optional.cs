using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class make_reciever_optional : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<int>(
			  name: "RecieverCounterPartyID",
			  table: "opt_paperorder",
			  type: "int(11)",
			  nullable: true,
			  oldClrType: typeof(int),
			  oldType: "int(11)");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{

		}
	}
}
