using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class change_wh_action_name : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update opt_WarehouseActionType set Name = 'Благотворительность/маркетинг/брак' where WarehouseActionTypeID = 5");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
