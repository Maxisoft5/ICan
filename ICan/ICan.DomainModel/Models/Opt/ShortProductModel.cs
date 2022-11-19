using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class ShortProductModel
	{
		public int ProductId { get; set; }

		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Display(Name = "Название")]
		public string Name { get; set; }

		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Display(Name = "Вид товара")]
		public int ProductKindId { get; set; }

		[Display(Name = "Набор")]
		public bool IsKit { get; set; }

		[Display(Name = "Доступен")]
		public bool Enabled { get; set; }
		public bool IsArchived { get; set; }
		public bool IsNoteBook => ProductKindId == 1;

		[Display(Name = "Серия")]
		public int? ProductSeriesId { get; set; }

		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Display(Name = "Ссылка на товар")]
		public string ProductUrl { get; set; }

		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Display(Name = "Вес, кг")]
		public float Weight { get; set; }

		[Display(Name = "Вид товара")]
		public string ProductKindName { get; set; }

		[Display(Name = "Серия")]
		public string ProductSeriesName { get; set; }

		[Display(Name = "ISBN")]
		[StringLength(13)]
		public string ISBN { get; set; }
	}
}
