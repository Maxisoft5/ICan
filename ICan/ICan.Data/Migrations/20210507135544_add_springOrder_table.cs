using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_springOrder_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "opt_springOrder",
                columns: table => new
                {
                    SpringOrderId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Provider = table.Column<string>(type: "varchar(100)", nullable: true),
                    Cost = table.Column<decimal>(nullable: false),
                    InvoiceNumber = table.Column<string>(type: "varchar(100)", nullable: true),
                    UPDNumber = table.Column<string>(type: "varchar(100)", nullable: true),
                    SpoolCount = table.Column<int>(nullable: false),
                    IsAssembled = table.Column<bool>(nullable: false),
                    SpringId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opt_springOrder", x => x.SpringOrderId);
                    table.ForeignKey(
                        name: "Opt_SpringOrder_SpringOrderId",
                        column: x => x.SpringId,
                        principalTable: "opt_spring",
                        principalColumn: "SpringId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_opt_springOrder_SpringId",
                table: "opt_springOrder",
                column: "SpringId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "opt_springOrder");
        }
    }
}
