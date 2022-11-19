using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_userfilename_image : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("UPDATE `opt_product` set `SiteEnabled`=1 where `ProductId` > 0 and `ProductKindId` = 1;", true);

			migrationBuilder.AddColumn<string>(
				name: "UserFileName",
				table: "opt_productimage",
				nullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "UserFileName",
				table: "opt_productimage");
		}
	}
}
