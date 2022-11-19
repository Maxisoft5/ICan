using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_is_arhived_report : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArhived",
                table: "opt_report",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
            migrationBuilder.Sql("update opt_report set IsArhived = 1  where ReportYear < 2021");


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArhived",
                table: "opt_report");
        }
    }
}
