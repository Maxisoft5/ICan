using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public partial class OptSemiproductType
	{
		public OptSemiproductType()

		{
			Semiproducts = new HashSet<OptSemiproduct>();
		}

		public int SemiproductTypeId { get; set; }

		public string Name { get; set; }

		public ICollection<OptSemiproduct> Semiproducts { get; set; }
	}
}
