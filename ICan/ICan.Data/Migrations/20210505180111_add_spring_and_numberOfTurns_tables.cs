using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_spring_and_numberOfTurns_tables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "opt_numberOfTurns",
                columns: table => new
                {
                    NumberOfTurnsId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NumberOfTurns = table.Column<int>(nullable: false),
                    Manufacturer = table.Column<string>(type: "varchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opt_numberOfTurns", x => x.NumberOfTurnsId);
                });

            migrationBuilder.CreateTable(
                name: "opt_springs",
                columns: table => new
                {
                    SpringId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SpringName = table.Column<string>(type: "varchar(100)", nullable: true),
                    BlockThickness = table.Column<int>(nullable: false),
                    Step = table.Column<int>(nullable: false),
                    NumberOfTurnsId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opt_springs", x => x.SpringId);
                    table.ForeignKey(
                        name: "Opt_NumberOfTurns_NumberOfTurnsId",
                        column: x => x.NumberOfTurnsId,
                        principalTable: "opt_numberOfTurns",
                        principalColumn: "NumberOfTurnsId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_opt_springs_NumberOfTurnsId",
                table: "opt_springs",
                column: "NumberOfTurnsId");

            migrationBuilder.Sql("insert into opt_numberOfTurns(NumberOfTurnsId, NumberOfTurns, Manufacturer)" +
                "values(1, 5000, 'Some Manufacturer'); ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "opt_springs");

            migrationBuilder.DropTable(
                name: "opt_numberOfTurns");
        }
    }
}
