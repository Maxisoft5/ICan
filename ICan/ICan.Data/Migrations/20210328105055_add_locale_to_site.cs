using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_locale_to_site : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<DateTime>(
				name: "Date",
				table: "opt_wbWarehouse",
				nullable: false,
				defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

			migrationBuilder.AddColumn<string>(
				name: "Locale",
				table: "opt_site",
				nullable: true);

			migrationBuilder.Sql(
				"update opt_sitefilter set SiteId = null where SiteId > 5;" +
				"delete from opt_site where siteId > 5;" +
				"update opt_site set Locale='ru-RU' where SiteId = 1; " +
				"update opt_site set Locale='en-GB' where SiteId = 2; " +
				"update opt_site set Locale='en-US' where SiteId = 3; " +
				"update opt_site set Locale='de-DE' where SiteId = 4; " +
				"update opt_site set Locale='es-ES' where SiteId = 5; ");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Date",
				table: "opt_wbWarehouse");

			migrationBuilder.DropColumn(
				name: "Locale",
				table: "opt_site");
		}
	}
}
