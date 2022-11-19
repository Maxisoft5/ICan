using ICan.Common.Domain;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class SemiproductShortModel
	{
		public int SemiproductId { get; set; }

		public int ProductId { get; set; }
		 
		public string Name { get; set; }

		public string DisplayName { get; set; }

		public string SemiproductTypeName { get; set; }
	}
}
