using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Domain
{
	public partial class OptProductkind
	{
		public OptProductkind()
		{
			OptProduct = new HashSet<OptProduct>();
		}

		public int ProductKindId { get; set; }
		[Display(Name = "Название")]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		public string Name { get; set; }


		public bool IsEditable { get; set; }

		public ICollection<OptProduct> OptProduct { get; set; }
	}
}
