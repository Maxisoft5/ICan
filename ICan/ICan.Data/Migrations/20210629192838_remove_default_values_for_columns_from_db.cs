using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class remove_default_values_for_columns_from_db : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Enabled",
                table: "opt_shop",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "'1'");

            migrationBuilder.AlterColumn<bool>(
                name: "IsKit",
                table: "opt_product",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "'0'");

            migrationBuilder.AlterColumn<bool>(
                name: "IsArchived",
                table: "opt_product",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "'1'");

            migrationBuilder.AlterColumn<bool>(
                name: "Enabled",
                table: "opt_product",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "'1'");

            migrationBuilder.AlterColumn<bool>(
                name: "Enabled",
                table: "opt_event",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "'1'");

            migrationBuilder.AlterColumn<bool>(
                name: "Enabled",
                table: "opt_discount",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "'1'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {         
            migrationBuilder.AlterColumn<bool>(
                name: "Enabled",
                table: "opt_shop",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "'1'",
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsKit",
                table: "opt_product",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "'0'",
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsArchived",
                table: "opt_product",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "'1'",
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>(
                name: "Enabled",
                table: "opt_product",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "'1'",
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>(
                name: "Enabled",
                table: "opt_event",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "'1'",
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>(
                name: "Enabled",
                table: "opt_discount",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "'1'",
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");
        }
    }
}
