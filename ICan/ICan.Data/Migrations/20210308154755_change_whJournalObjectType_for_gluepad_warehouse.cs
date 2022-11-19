using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class change_whJournalObjectType_for_gluepad_warehouse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update opt_warehousetype set WarehouseObjectType = 4 where WarehouseTypeId = 8;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
