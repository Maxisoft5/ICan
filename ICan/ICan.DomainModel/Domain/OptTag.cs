using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public class OptTag
	{
		public OptTag()
		{
			ProductTags = new HashSet<OptProductTag>();
		}

		public int TagId { get; set; }
		public string TagName { get; set; }

		public ICollection<OptProductTag> ProductTags { get; set; }
	}
}
