using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_amazon_marketplace : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(
			"INSERT INTO opt_marketplace (`MarketplaceId`, `Name`) VALUES(4, 'AmazonUK'); ", true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(
				  "DELETE FROM opt_marketplace WHERE MarketPlaceId = 4", true);
		}
	}
}
