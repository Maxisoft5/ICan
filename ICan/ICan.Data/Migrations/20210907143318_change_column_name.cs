using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class change_column_name : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CampaingName",
                table: "opt_campaign",
                newName: "CampaignName");

            migrationBuilder.RenameColumn(
                name: "CampainType",
                table: "opt_campaign",
                newName: "CampaignType");

            migrationBuilder.RenameColumn(
                name: "CampaingId",
                table: "opt_campaign",
                newName: "CampaignId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CampaignType",
                table: "opt_campaign",
                newName: "CampainType");

            migrationBuilder.RenameColumn(
                name: "CampaignName",
                table: "opt_campaign",
                newName: "CampaingName");

            migrationBuilder.RenameColumn(
                name: "CampaignId",
                table: "opt_campaign",
                newName: "CampaingId");
        }
    }
}
