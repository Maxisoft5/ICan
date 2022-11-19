using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public partial class OptOrderstatus
	{
		public OptOrderstatus()
		{
			//OptOrder = new HashSet<OptOrder>();
		}

		public int OrderStatusId { get; set; }
		public string Name { get; set; }

		public ICollection<OptOrder> OptOrder { get; set; }
	}
}
