using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_applicationUSer_Shop_Relation_many_to_Many : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "opt_applicationusershoprelation",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    ShopId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opt_applicationusershoprelation", x => new { x.ShopId, x.UserId });
                    table.ForeignKey(
                        name: "FK_opt_applicationusershoprelation_opt_shop_ShopId",
                        column: x => x.ShopId,
                        principalTable: "opt_shop",
                        principalColumn: "ShopID",
                        onDelete: ReferentialAction.Cascade);
                    //table.ForeignKey(
                    //    name: "FK_opt_applicationusershoprelation_aspnetusers_UserId",
                    //    column: x => x.UserId,
                    //    principalTable: "aspnetusers",
                    //    principalColumn: "Id",
                    //    onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_opt_applicationusershoprelation_UserId",
                table: "opt_applicationusershoprelation",
                column: "UserId");

            migrationBuilder.Sql("ALTER TABLE  `opt_applicationusershoprelation` " +
                "CHANGE COLUMN `UserId` `UserId` VARCHAR(255) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci' NOT NULL");

            migrationBuilder.Sql("ALTER TABLE  `opt_applicationusershoprelation` " +
                                "ADD CONSTRAINT `FK_opt_applicationusershoprelation_opt_Aspnetusers_Id` " +
                                "FOREIGN KEY(`UserId`) " +
                                "REFERENCES `aspnetusers` (`Id`) " +
                                "ON DELETE CASCADE " +
                                "ON UPDATE CASCADE;");

            migrationBuilder.Sql("insert into opt_applicationusershoprelation(ShopId,UserId)" +
                                "select ShopId, ClientId from opt_shop where ClientId is not null");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "opt_applicationusershoprelation");
        }
    }
}
