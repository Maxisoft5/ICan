using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_order_fields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryPointAddress",
                table: "opt_order",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShopID",
                table: "opt_order",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_opt_order_ShopId",
                table: "opt_order",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_ShopID",
                table: "opt_order",
                column: "ShopID",
                principalTable: "opt_shop",
                principalColumn: "ShopID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_ShopID",
                table: "opt_order");

            migrationBuilder.DropIndex(
                name: "IX_opt_order_ShopId",
                table: "opt_order");

            migrationBuilder.DropColumn(
                name: "DeliveryPointAddress",
                table: "opt_order");

            migrationBuilder.DropColumn(
                name: "ShopID",
                table: "opt_order");
        }
    }
}
