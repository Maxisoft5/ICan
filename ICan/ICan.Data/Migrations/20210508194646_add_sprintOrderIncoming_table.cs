using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_sprintOrderIncoming_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Opt_SpringOrder_SpringOrderId",
                table: "opt_springOrder");

            migrationBuilder.CreateTable(
                name: "opt_springOrderIncoming",
                columns: table => new
                {
                    SpringOrderIncomingId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SpoolCount = table.Column<int>(nullable: false),
                    NumberOfTurnsCount = table.Column<int>(nullable: false),
                    IncomingDate = table.Column<DateTime>(nullable: false),
                    SpringOrderId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opt_springOrderIncoming", x => x.SpringOrderIncomingId);
                    table.ForeignKey(
                        name: "Opt_SpringOrder_SpringOrderId",
                        column: x => x.SpringOrderId,
                        principalTable: "opt_springOrder",
                        principalColumn: "SpringOrderId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_opt_springOrderIncoming_SpringOrderId",
                table: "opt_springOrderIncoming",
                column: "SpringOrderId");

            migrationBuilder.AddForeignKey(
                name: "Opt_Spring_SpringId",
                table: "opt_springOrder",
                column: "SpringId",
                principalTable: "opt_spring",
                principalColumn: "SpringId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Opt_Spring_SpringId",
                table: "opt_springOrder");

            migrationBuilder.DropTable(
                name: "opt_springOrderIncoming");

            migrationBuilder.AddForeignKey(
                name: "Opt_SpringOrder_SpringOrderId",
                table: "opt_springOrder",
                column: "SpringId",
                principalTable: "opt_spring",
                principalColumn: "SpringId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
