using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public class MaterialImage
	{
		public OptMaterial  Material { get; set; }
		public IEnumerable<OptImage> Images { get;set; }
	}
}
