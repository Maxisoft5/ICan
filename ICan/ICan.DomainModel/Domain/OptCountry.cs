using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public class OptCountry
	{
		public OptCountry()
		{
			Products = new HashSet<OptProduct>();
		}

		public int CountryId { get; set; }

		public string Name { get; set; }
		
		public string Prefix { get; set; }
	
		public ICollection<OptProduct> Products { get; set; }
	}
}
