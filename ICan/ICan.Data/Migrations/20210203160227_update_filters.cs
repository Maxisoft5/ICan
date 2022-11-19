using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class update_filters : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@"ALTER TABLE  
                        `opt_sitefilterproduct` 
                        DROP FOREIGN KEY `Opt_SiteFilterProduct_SiteFilterId`;
                          ALTER TABLE `opt_sitefilterproduct` 
                        ADD CONSTRAINT `Opt_SiteFilterProduct_SiteFilterId`
                          FOREIGN KEY(`SiteFilterId`)
                          REFERENCES `opt_sitefilter` (`SiteFilterId`)
                          ON DELETE CASCADE
                          ON UPDATE CASCADE;
            ", true);
			migrationBuilder.Sql("UPDATE `opt_sitefilter` SET `SiteFilterId` = '6' WHERE (`SiteFilterId` = '5');", true);
			migrationBuilder.Sql("UPDATE `opt_sitefilter` SET `SiteFilterId` = '5' WHERE (`SiteFilterId` = '4');", true);
			migrationBuilder.Sql("UPDATE `opt_sitefilter` SET `SiteFilterId` = '4' WHERE (`SiteFilterId` = '3');", true);
			migrationBuilder.Sql("UPDATE `opt_sitefilter` SET `SiteFilterId` = '3' WHERE (`SiteFilterId` = '2');", true);
			migrationBuilder.Sql("UPDATE `opt_sitefilter` SET `SiteFilterId` = '2' WHERE (`SiteFilterId` = '1');", true);
			migrationBuilder.Sql("UPDATE `opt_sitefilter` SET `SiteFilterId` = '1' WHERE (`SiteFilterId` = '999');", true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{

		}
	}
}
