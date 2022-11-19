using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_dateTime_for_springOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Opt_SpringOrder_SpringOrderId",
                table: "opt_springOrderIncoming");

            migrationBuilder.AddColumn<DateTime>(
                name: "OrderDate",
                table: "opt_springOrder",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "Opt_SpringOrder_SpringOrderId",
                table: "opt_springOrderIncoming",
                column: "SpringOrderId",
                principalTable: "opt_springOrder",
                principalColumn: "SpringOrderId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Opt_SpringOrder_SpringOrderId",
                table: "opt_springOrderIncoming");

            migrationBuilder.DropColumn(
                name: "OrderDate",
                table: "opt_springOrder");

            migrationBuilder.AddForeignKey(
                name: "Opt_SpringOrder_SpringOrderId",
                table: "opt_springOrderIncoming",
                column: "SpringOrderId",
                principalTable: "opt_springOrder",
                principalColumn: "SpringOrderId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
