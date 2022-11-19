using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_glue_pad_to_products_table : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("insert into opt_productKind(ProductKindID,Name,IsEditable) values(3, 'Клеевые подушки', 0); ");
			migrationBuilder.Sql("insert into opt_product(ProductID, Name, ProductKindID,Enabled, Weight) values(100, 'Клеевые подушки', 3, false, 0); ");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("delete from opt_product where ProductKindID = 3;");
			migrationBuilder.Sql("delete from opt_productKind where ProductKindID = 3; ");
		}
	}
}
