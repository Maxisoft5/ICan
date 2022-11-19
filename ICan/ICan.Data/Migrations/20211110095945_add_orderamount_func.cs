using Microsoft.EntityFrameworkCore.Migrations;

namespace ICan.Data.Migrations
{
	public partial class add_orderamount_func : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@" 
					 CREATE FUNCTION getOrderAmount (productId int, orderYear int,  
					 orderMonth int)
					  RETURNS int  
					  BEGIN
					  DECLARE amount int(20) DEFAULT 0;	
						select sum(op.Amount) into amount  from opt_order o join opt_orderproduct op
						   on op.OrderId = o.OrderId 
						   where o.OrderStatusId = 3 and 
						   EXTRACT(month from o.DoneDate) = orderMonth and 
						   EXTRACT(year from o.DoneDate) = orderYear and 
						   op.ProductId =  productId;
						   return amount;
					END;");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{

		}
	}
}
