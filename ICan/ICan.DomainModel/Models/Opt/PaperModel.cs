using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class PaperModel
	{
		public int PaperId { get; set; }

		[Display(Name = "Название")]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		public string Name { get; set; }

		[Display(Name = "Описание")]
		public string Description { get; set; }

		[Display(Name = "Вид бумаги")]
		public int? TypeOfPaperId { get; set; }

		[Display(Name = "Вид бумаги")]
		public string TypeOfPaperDisplayName => TypeOfPaper?.Name;

		[Display(Name = "Ш, мм")]
		public int? Width { get; set; }

		[Display(Name = "Д, мм")]
		public int? Length { get; set; }

		public TypeOfPaperModel TypeOfPaper { get; set; }
		public ICollection<SemiproductModel> Semiproducts { get; set; }
		public ICollection<PaperOrderModel> PaperOrders { get; set; }
	}
}
