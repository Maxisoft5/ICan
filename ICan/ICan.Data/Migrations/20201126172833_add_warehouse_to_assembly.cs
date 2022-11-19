using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_warehouse_to_assembly : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                 name: "Opt_Warehouse_AssemblyID",
                 table: "opt_warehouse");

            migrationBuilder.DropIndex(
                name: "Opt_Warehouse_AssemblyID_idx",
                table: "opt_warehouse");

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "opt_assembly",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_opt_assembly_WarehouseId",
                table: "opt_assembly",
                column: "WarehouseId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "Opt_Assembly_WarehouseID",
                table: "opt_assembly",
                column: "WarehouseId",
                principalTable: "opt_warehouse",
                principalColumn: "WarehouseId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql(
               @"CREATE PROCEDURE `SetWarehoudeId`()
	            BEGIN
	              DECLARE done bit default false;
	              DECLARE whId, asId INT(20) ;
	 
	              DECLARE cur CURSOR FOR 
		            SELECT  AssemblyId, WarehouseId FROM opt_warehouse   where AssemblyID is not null;
		            DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;
		            Open cur;
			            WHILE not done  DO 
			             FETCH cur  INTO asId, whId;
			            UPDATE opt_assembly 
	            SET 
		            WarehouseId = whId
	            WHERE
		            AssemblyID = asId;
			            END WHILE;
		            Close cur; 
	            END
            ", true);

            migrationBuilder.Sql(" call SetWarehoudeId();", true);
            migrationBuilder.Sql(" drop procedure SetWarehoudeId;", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Opt_Assembly_WarehouseID",
                table: "opt_assembly");

            migrationBuilder.DropIndex(
                name: "IX_opt_assembly_WarehouseId",
                table: "opt_assembly");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "opt_assembly");

            migrationBuilder.CreateIndex(
                name: "IX_opt_warehouse_AssemblyId",
                table: "opt_warehouse",
                column: "AssemblyId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_opt_warehouse_opt_assembly_AssemblyId",
                table: "opt_warehouse",
                column: "AssemblyId",
                principalTable: "opt_assembly",
                principalColumn: "AssemblyID",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
