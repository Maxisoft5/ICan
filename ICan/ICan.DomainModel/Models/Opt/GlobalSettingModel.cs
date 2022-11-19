using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class GlobalSettingModel
	{
		public int GlobalSettingId { get; set; }

		[Required]
		[StringLength(1000)]
		[Display(Name ="Содержание")]
		public string Content { get; set; }
		[Display(Name = "Комментарий")]
		public string Comment { get; set; }
	}
}
