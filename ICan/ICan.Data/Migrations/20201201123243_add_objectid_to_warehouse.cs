using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_objectid_to_warehouse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssemblyId",
                table: "opt_warehouse");

            migrationBuilder.AlterColumn<int>(
                name: "ProductID",
                table: "opt_warehouseitem",
                type: "int(11)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int(11)");

            migrationBuilder.AddColumn<int>(
                name: "ObjectId",
                table: "opt_warehouseitem",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ObjectId",
                table: "opt_warehouseitem");

            migrationBuilder.AlterColumn<int>(
                name: "ProductID",
                table: "opt_warehouseitem",
                type: "int(11)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int(11)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssemblyId",
                table: "opt_warehouse",
                type: "int",
                nullable: true);
        }
    }
}
