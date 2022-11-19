using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_site_fk_data : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
              name: "IX_opt_sitefilter_SiteId",
              table: "opt_sitefilter",
              column: "SiteId");

            migrationBuilder.AddForeignKey(
                name: "Opt_SiteFilter_SiteId",
                table: "opt_sitefilter",
                column: "SiteId",
                principalTable: "opt_site",
                principalColumn: "SiteId",
                onDelete: ReferentialAction.Restrict);


            migrationBuilder.Sql(
                " INSERT INTO opt_sitefilter (`SiteId`, `FilterCode`, `Name`) VALUES(2, 1, 'Главная страница');" +
                " INSERT INTO opt_sitefilter (`SiteId`, `FilterCode`, `Name`) VALUES(2, 2, '2-3');" +
                " INSERT INTO opt_sitefilter (`SiteId`, `FilterCode`, `Name`) VALUES(2, 3, '3-4');" +

                " INSERT INTO opt_sitefilter (`SiteId`, `FilterCode`, `Name`) VALUES(3, 1, 'Главная страница');" +
                " INSERT INTO opt_sitefilter (`SiteId`, `FilterCode`, `Name`) VALUES(3, 2, '2-3');" +
                " INSERT INTO opt_sitefilter (`SiteId`, `FilterCode`, `Name`) VALUES(3, 3, '3-4');" +

                " INSERT INTO opt_sitefilter (`SiteId`, `FilterCode`, `Name`) VALUES(4, 1, 'Главная страница');" +
                " INSERT INTO opt_sitefilter (`SiteId`, `FilterCode`, `Name`) VALUES(4, 2, '2-3');" +
                " INSERT INTO opt_sitefilter (`SiteId`, `FilterCode`, `Name`) VALUES(4, 3, '3-4');" +

                " INSERT INTO opt_sitefilter (`SiteId`, `FilterCode`, `Name`) VALUES(5, 1, 'Главная страница');" +
                " INSERT INTO opt_sitefilter (`SiteId`, `FilterCode`, `Name`) VALUES(5, 2, '2-3');" +
                " INSERT INTO opt_sitefilter (`SiteId`, `FilterCode`, `Name`) VALUES(5, 3, '3-4');");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
