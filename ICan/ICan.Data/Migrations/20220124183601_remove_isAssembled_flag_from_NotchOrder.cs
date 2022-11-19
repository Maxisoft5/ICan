using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class remove_isAssembled_flag_from_NotchOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update opt_notchorderincomingitem " +
                "set IsAssembled = true " +
                "where NotchOrderIncomingID in (select NotchOrderIncomingID " +
                "                                from opt_notchorderincoming incoming " +
                "                                join opt_notchorder ordr " +
                "                                on incoming.NotchOrderId = ordr.NotchOrderID " +
                "                                where IsAssembled = true);");
            migrationBuilder.DropColumn(
                name: "IsAssembled",
                table: "opt_notchorder");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAssembled",
                table: "opt_notchorder",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
