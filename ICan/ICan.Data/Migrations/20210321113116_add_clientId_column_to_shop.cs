using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_clientId_column_to_shop : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE  `opt_shop` " +
                "ADD COLUMN `ClientId` VARCHAR(150) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci' NULL AFTER `MimeType`, " +
                "ADD INDEX `Opt_Shop_ClientId_idx` (`ClientId` ASC) VISIBLE; " +
                "ALTER TABLE  `opt_shop`  " +
                "ADD CONSTRAINT `Opt_Shop_ClientId` " +
                  "FOREIGN KEY(`ClientId`) " +
                  "REFERENCES `aspnetusers` (`Id`) " +
                  "ON DELETE RESTRICT;");

            migrationBuilder.DropForeignKey(
                name: "Opt_Aspenetusers_ShopId",
                table: "aspnetusers");

            //migrationBuilder.AddColumn<string>(
            //    name: "ClientId",
            //    table: "opt_shop",
            //    maxLength: 150,
            //    nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_opt_shop_ClientId",
                table: "opt_shop",
                column: "ClientId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_aspnetusers_opt_shop_ShopID",
            //    table: "aspnetusers",
            //    column: "ShopID",
            //    principalTable: "opt_shop",
            //    principalColumn: "ShopID",
            //    onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "Opt_Shop_ClientId",
            //    table: "opt_shop",
            //    column: "ClientId",
            //    principalTable: "aspnetusers",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_aspnetusers_opt_shop_ShopID",
                table: "aspnetusers");

            migrationBuilder.DropForeignKey(
                name: "Opt_Shop_ClientId",
                table: "opt_shop");

            migrationBuilder.DropIndex(
                name: "IX_opt_shop_ClientId",
                table: "opt_shop");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "opt_shop");

            migrationBuilder.AddForeignKey(
                name: "Opt_Aspenetusers_ShopId",
                table: "aspnetusers",
                column: "ShopID",
                principalTable: "opt_shop",
                principalColumn: "ShopID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
