using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_reviews_amount : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<float>(
				name: "Raiting",
				table: "opt_marketplaceproduct",
				nullable: true,
				oldClrType: typeof(int),
				oldType: "int",
				oldNullable: true);

			migrationBuilder.AddColumn<int>(
				name: "ReviewsAmount",
				table: "opt_marketplaceproduct",
				nullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "ReviewsAmount",
				table: "opt_marketplaceproduct");

			migrationBuilder.AlterColumn<int>(
				name: "Raiting",
				table: "opt_marketplaceproduct",
				type: "int",
				nullable: true,
				oldClrType: typeof(float),
				oldNullable: true);
		}
	}
}
