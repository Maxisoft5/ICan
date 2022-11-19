using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_marketplace_table : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "opt_marketplace",
				columns: table => new
				{
					MarketplaceId = table.Column<int>(nullable: false)
						.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
					Name = table.Column<string>(type: "varchar(50)", nullable: false),
					ImageName = table.Column<string>(nullable: true),
					Url = table.Column<string>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_opt_marketplace", x => x.MarketplaceId);
				});
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "opt_marketplace");
		}
	}
}
