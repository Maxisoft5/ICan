using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class ShopNameModel
	{
		public int ShopNameId { get; set; }
		public int ShopId { get; set; }
		public string Shop { get; set; }

		[Required]
		[Display(Name = "Название юр лица (Грузополучатель)")]
		public string Name { get; set; }

		[Display(Name = "ИНН")]
		public string Inn { get; set; }

		[Display(Name = "Активно")]
		public bool Enabled { get; set; }
	}
}
