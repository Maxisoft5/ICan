using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class set_ClientId_for_Shops_and_delete_shopId_column : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update opt_shop, (select Id, ShopId from aspnetusers where ShopId is not null) as tb " +
                                    "set ClientId = tb.Id " +
                                    "where opt_shop.ShopId = tb.ShopId;");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_aspnetusers_opt_shop_ShopID",
            //    table: "aspnetusers");

            //migrationBuilder.DropIndex(
            //    name: "IX_aspnetusers_ShopID",
            //    table: "aspnetusers");

            migrationBuilder.DropColumn(
                name: "ShopID",
                table: "aspnetusers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShopID",
                table: "aspnetusers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_aspnetusers_ShopID",
                table: "aspnetusers",
                column: "ShopID");

            migrationBuilder.AddForeignKey(
                name: "FK_aspnetusers_opt_shop_ShopID",
                table: "aspnetusers",
                column: "ShopID",
                principalTable: "opt_shop",
                principalColumn: "ShopID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
