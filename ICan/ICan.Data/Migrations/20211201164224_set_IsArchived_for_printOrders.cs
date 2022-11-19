using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class set_IsArchived_for_printOrders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update opt_printorder set IsArchived = true where OrderDate <= '2019-12-31'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
