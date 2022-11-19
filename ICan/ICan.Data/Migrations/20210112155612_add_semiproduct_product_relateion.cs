using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_semiproduct_product_relateion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUniversal",
                table: "opt_semiproduct",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "opt_semiproductproductrelation",
                columns: table => new
                {
                    SemiproductId = table.Column<int>(nullable: false),
                    ProductId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opt_semiproductproductrelation", x => new { x.SemiproductId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_opt_semiproductproductrelation_opt_product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "opt_product",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_opt_semiproductproductrelation_opt_semiproduct_SemiproductId",
                        column: x => x.SemiproductId,
                        principalTable: "opt_semiproduct",
                        principalColumn: "SemiproductID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_opt_semiproductproductrelation_ProductId",
                table: "opt_semiproductproductrelation",
                column: "ProductId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "opt_semiproductproductrelation");

            migrationBuilder.DropColumn(
                name: "IsUniversal",
                table: "opt_semiproduct");
        }
    }
}
