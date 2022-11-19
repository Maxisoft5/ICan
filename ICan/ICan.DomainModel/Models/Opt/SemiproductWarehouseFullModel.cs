namespace ICan.Common.Models.Opt
{
	public class SemiproductWarehouseFullModel
	{
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public bool IsKit { get; set; }

		public int? BlockId { get; set; }
		public int? BlockAmount { get; set; }

		public int? StickersId { get; set; }
		public int? StickersAmount { get; set; }
		public int? CoversId { get; set; }
		public int? CoversAmount { get; set; }

		//Крышка
		public int? BoxFrontId { get; set; }
		public int? BoxFrontAmount { get; set; }

		//Дно
		public int? BoxBackId { get; set; }
		public int? BoxBackAmount { get; set; }

		public int? CursorId { get; set; }
		public int? CursorAmount { get; set; }

		public int SeriesId { get; set; }
		public string SeriesName { get; set; }
		public int? PointerId { get; set; }
	}
}
