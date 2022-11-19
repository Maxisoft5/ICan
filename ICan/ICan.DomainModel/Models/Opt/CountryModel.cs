using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class CountryModel
	{
		public int CountryId { get; set; }

		[Display(Name = "Название")]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		public string Name { get; set; }

		[Display(Name = "Префикс")]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		public string Prefix { get; set; }
	}
}
