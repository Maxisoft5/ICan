using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class ProductSeriesModel
	{
		public int ProductSeriesId { get; set; }


		[Display(Name = "Вид товара")]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		public int ProductKindId { get; set; } = 1;


		[Display(Name = "Название")]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		public string Name { get; set; }

		[Display(Name = "Название для сайта")]
		public string SiteName { get; set; }

		[Display(Name = "Порядок отображения")]
		public int? Order { get; set; }

	}
}
