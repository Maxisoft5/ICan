using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_notchorder_to_assembly : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PrintOrderSemiproductId",
                table: "opt_assemblysemiproduct",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "NotchOrderId",
                table: "opt_assemblysemiproduct",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_opt_assemblysemiproduct_NotchOrderId",
                table: "opt_assemblysemiproduct",
                column: "NotchOrderId");

            migrationBuilder.AddForeignKey(
                name: "Opt_AssemblySemiproduct_NotchOrderID",
                table: "opt_assemblysemiproduct",
                column: "NotchOrderId",
                principalTable: "opt_notchorder",
                principalColumn: "NotchOrderId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Opt_AssemblySemiproduct_NotchOrderID",
                table: "opt_assemblysemiproduct");

            migrationBuilder.DropIndex(
                name: "IX_opt_assemblysemiproduct_NotchOrderId",
                table: "opt_assemblysemiproduct");

            migrationBuilder.DropColumn(
                name: "NotchOrderId",
                table: "opt_assemblysemiproduct");

            migrationBuilder.AlterColumn<int>(
                name: "PrintOrderSemiproductId",
                table: "opt_assemblysemiproduct",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
