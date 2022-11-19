using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_warehouse_type : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Opt_Warehouse_ShopID",
                table: "opt_warehouse");

            migrationBuilder.DropIndex(
                name: "Opt_Warehouse_ShopID_idx",
                table: "opt_warehouse");

            migrationBuilder.DropColumn(
                name: "ShopID",
                table: "opt_warehouse");

            migrationBuilder.AddColumn<int>(
                name: "WarehouseTypeId",
                table: "opt_warehousejournal",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseTypeId",
                table: "opt_warehouse",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "opt_warehousetype",
                columns: table => new
                {
                    WarehouseTypeId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Comment = table.Column<string>(nullable: true),
                    ReadyToUse = table.Column<bool>(nullable: false, defaultValue: false),
                    WarehouseObjectType = table.Column<int>(nullable: false),
                    CounterpartyId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opt_warehousetype", x => x.WarehouseTypeId);
                    table.ForeignKey(
                        name: "Opt_WarehouseType_CounterpartyID",
                        column: x => x.CounterpartyId,
                        principalTable: "opt_counterparty",
                        principalColumn: "CounterpartyID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_opt_warehousejournal_WarehouseTypeId",
                table: "opt_warehousejournal",
                column: "WarehouseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_opt_warehouse_WarehouseTypeId",
                table: "opt_warehouse",
                column: "WarehouseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_opt_warehousetype_CounterpartyId",
                table: "opt_warehousetype",
                column: "CounterpartyId");

            migrationBuilder.AddForeignKey(
                name: "Opt_Warehouse_WarehouseTypeID",
                table: "opt_warehouse",
                column: "WarehouseTypeId",
                principalTable: "opt_warehousetype",
                principalColumn: "WarehouseTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "Opt_WarehouseJournal_WarehouseTypeID",
                table: "opt_warehousejournal",
                column: "WarehouseTypeId",
                principalTable: "opt_warehousetype",
                principalColumn: "WarehouseTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("INSERT INTO opt_warehousetype (`WarehouseTypeId`, `Name`, `WarehouseObjectType`, `ReadyToUse`) VALUES(1, 'Готовые тетради', 1, 1); " +
        "INSERT INTO opt_warehousetype (`WarehouseTypeId`, `Name`, `WarehouseObjectType`, `ReadyToUse`) VALUES(2, 'Готовые полуфабрикаты', 2, 1); " +
        "INSERT INTO opt_warehousetype (`WarehouseTypeId`, `Name`, `WarehouseObjectType`, `ReadyToUse`) VALUES(3, 'Готовая бумага', 3, 1); ", true);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Opt_Warehouse_WarehouseTypeID",
                table: "opt_warehouse");

            migrationBuilder.DropForeignKey(
                name: "Opt_WarehouseJournal_WarehouseTypeID",
                table: "opt_warehousejournal");

            migrationBuilder.DropTable(
                name: "opt_warehousetype");

            migrationBuilder.DropIndex(
                name: "IX_opt_warehousejournal_WarehouseTypeId",
                table: "opt_warehousejournal");

            migrationBuilder.DropIndex(
                name: "IX_opt_warehouse_WarehouseTypeId",
                table: "opt_warehouse");

            migrationBuilder.DropColumn(
                name: "WarehouseTypeId",
                table: "opt_warehousejournal");

            migrationBuilder.DropColumn(
                name: "WarehouseTypeId",
                table: "opt_warehouse");

            migrationBuilder.AddColumn<int>(
                name: "ShopID",
                table: "opt_warehouse",
                type: "int(11)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "Opt_Warehouse_ShopID_idx",
                table: "opt_warehouse",
                column: "ShopID");

            migrationBuilder.AddForeignKey(
                name: "Opt_Warehouse_ShopID",
                table: "opt_warehouse",
                column: "ShopID",
                principalTable: "opt_shop",
                principalColumn: "ShopID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
