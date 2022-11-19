using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_cursor_type : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("insert into opt_semiproducttype (SemiproductTypeID, Name) values (5, 'Курсоры');");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("delete from opt_semiproducttype where SemiproductTypeID = 5;");
		}
	}
}
