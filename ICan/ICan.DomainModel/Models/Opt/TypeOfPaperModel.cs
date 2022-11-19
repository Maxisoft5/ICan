using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class TypeOfPaperModel
	{
		public int TypeOfPaperId { get; set; }
		[Display(Name = "Название вида бумаги")]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		public string Name { get; set; }
		[Display(Name = "Тип бумаги")]
		public string PaperType { get; set; }
		[Display(Name = "Плотность, гр/м2")]
		public int? Density { get; set; }
	}
}
