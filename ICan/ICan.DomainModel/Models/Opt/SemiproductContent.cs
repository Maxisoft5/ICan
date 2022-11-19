namespace ICan.Common.Models.Opt
{
	public class SemiproductContent
	{
		public int? BlockId { get; set; }
		public int? StickersId { get; set; }
		public int? CoversId { get; set; }
		
		//Крышка
		public int? BoxFrontId { get; set; }
		
		//Дно
		public int? BoxBackId { get; set; }

		public int? CursorId { get; set; }
		public int? PointerId { get; set; }
	}
}
