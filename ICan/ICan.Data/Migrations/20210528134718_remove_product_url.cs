using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class remove_product_url : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "ProductUrl",
				table: "opt_product");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "ProductUrl",
				table: "opt_product",
				type: "varchar(200)",
				maxLength: 200,
				nullable: true)
				.Annotation("MySql:CharSet", "utf8mb4");
		}
	}
}
