using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_blockTypeId_to_Semiproduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BlockTypeId",
                table: "opt_semiproduct",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_opt_semiproduct_BlockTypeId",
                table: "opt_semiproduct",
                column: "BlockTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_opt_semiproduct_opt_blocktype_BlockTypeId",
                table: "opt_semiproduct",
                column: "BlockTypeId",
                principalTable: "opt_blocktype",
                principalColumn: "BlockTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_opt_semiproduct_opt_blocktype_BlockTypeId",
                table: "opt_semiproduct");

            migrationBuilder.DropIndex(
                name: "IX_opt_semiproduct_BlockTypeId",
                table: "opt_semiproduct");

            migrationBuilder.DropColumn(
                name: "BlockTypeId",
                table: "opt_semiproduct");
        }
    }
}
