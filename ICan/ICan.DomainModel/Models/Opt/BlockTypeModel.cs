using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class BlockTypeModel
	{
		[Display(Name = "Тип блока")]
		public int BlockTypeId { get; set; }
		public string Name { get; set; }
	}
}
