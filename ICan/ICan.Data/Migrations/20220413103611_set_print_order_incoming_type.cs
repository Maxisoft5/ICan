using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class set_print_order_incoming_type : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("update opt_printorderincoming set IncomingType = 1 where PrintOrderIncomingId > 0;");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{

		}
	}
}
