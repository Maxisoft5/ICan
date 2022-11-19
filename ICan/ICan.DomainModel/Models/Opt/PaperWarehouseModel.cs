using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class PaperWarehouseModel
	{
		public int PaperId { get; set; }

		[Display(Name = "Название")]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		public string Name { get; set; }

		[Display(Name = "Описание")]
		public string Description { get; set; }

		[Display(Name = "Вид бумаги")]
		public int? TypeOfPaperId { get; set; }

		[Display(Name = "Кол-во бумаги")]
		public int? Amount { get; set; }

		[Display(Name = "Вид бумаги")]
		public string TypeOfPaperDisplayName { get; set; }
	}
}
