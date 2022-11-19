using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class rename_field_for_campaign : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MailChimpCampaignId",
                table: "opt_campaign",
                newName: "ExternalCampaignId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExternalCampaignId",
                table: "opt_campaign",
                newName: "MailChimpCampaignId");
        }
    }
}
