using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_pointer_semiproduct_type : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO opt_semiproducttype (`SemiproductTypeID`, `Name`) VALUES(6, 'Пойнтер');");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
