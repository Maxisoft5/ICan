using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_site_for_filters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FilterCode",
                table: "opt_sitefilter",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SiteId",
                table: "opt_sitefilter",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "opt_site",
                columns: table => new
                {
                    SiteId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opt_site", x => x.SiteId);
                });

            migrationBuilder.Sql("INSERT INTO opt_site (`SiteId`, `Name`) VALUES(1, 'yamogu.ru')");
            migrationBuilder.Sql("INSERT INTO opt_site (`SiteId`, `Name`) VALUES(2, 'yamogu.uk')");
            migrationBuilder.Sql("INSERT INTO opt_site (`SiteId`, `Name`) VALUES(3, 'yamogu.com')");
            migrationBuilder.Sql("INSERT INTO opt_site (`SiteId`, `Name`) VALUES(4, 'yamogu.de')");
            migrationBuilder.Sql("INSERT INTO opt_site (`SiteId`, `Name`) VALUES(5, 'yamogu.es')");

            migrationBuilder.Sql("UPDATE opt_sitefilter SET FilterCode = SiteFilterId, SiteId = 1 where IsInternal = 1;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Opt_SiteFilter_SiteId",
                table: "opt_sitefilter");

            migrationBuilder.DropTable(
                name: "opt_site");

            migrationBuilder.DropIndex(
                name: "IX_opt_sitefilter_SiteId",
                table: "opt_sitefilter");

            migrationBuilder.DropColumn(
                name: "FilterCode",
                table: "opt_sitefilter");

            migrationBuilder.DropColumn(
                name: "SiteId",
                table: "opt_sitefilter");
        }
    }
}
