using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_new_role : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("INSERT INTO  `aspnetroles` (`Id`, `Name`, `NormalizedName`) VALUES ('contentman', 'ContentMan', 'CONTENTMAN');", true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
		 
		}
	}
}
