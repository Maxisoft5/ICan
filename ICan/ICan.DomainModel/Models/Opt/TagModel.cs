using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class TagModel
	{
		public int TagId { get; set; }
		[Display(Name="Название тега")]
		public string TagName { get; set; }
		public int OrderNumber { get; set; }
	}
}
