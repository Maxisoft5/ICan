using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
    public partial class add_blockType_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "opt_blocktype",
                columns: table => new
                {
                    BlockTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opt_blocktype", x => x.BlockTypeId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
      

            migrationBuilder.Sql("insert into opt_blocktype(BlockTypeId, Name)" +
                "values" +
                "(1, 'Блок 150 гр')," +
                "(2, 'Блок 170 гр')," +
                "(3, 'Блок 115 гр')," +
                "(4, 'Блок 5х150 гр')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        { 
            migrationBuilder.DropTable(
                name: "opt_blocktype");       
        }
    }
}
