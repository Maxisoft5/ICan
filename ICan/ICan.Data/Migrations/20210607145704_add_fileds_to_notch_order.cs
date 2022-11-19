using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_fileds_to_notch_order : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SemiproductId",
                table: "opt_notchorderincomingitem",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "NotchOrderItemStickerId",
                table: "opt_notchorderincomingitem",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_opt_notchordersticker_SemiproductId",
                table: "opt_notchordersticker",
                column: "SemiproductId");

            migrationBuilder.AddForeignKey(
                name: "Opt_NotchOrderSticker_SemiproductId",
                table: "opt_notchordersticker",
                column: "SemiproductId",
                principalTable: "opt_semiproduct",
                principalColumn: "SemiproductID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Opt_NotchOrderSticker_SemiproductId",
                table: "opt_notchordersticker");

            migrationBuilder.DropIndex(
                name: "IX_opt_notchordersticker_SemiproductId",
                table: "opt_notchordersticker");

            migrationBuilder.DropColumn(
                name: "NotchOrderItemStickerId",
                table: "opt_notchorderincomingitem");

            migrationBuilder.AlterColumn<int>(
                name: "SemiproductId",
                table: "opt_notchorderincomingitem",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
