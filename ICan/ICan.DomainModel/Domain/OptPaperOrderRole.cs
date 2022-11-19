using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public partial class OptPaperOrderRole
	{
		public OptPaperOrderRole()
		{
			CounterParties = new HashSet<OptCounterparty>();
		}

		public int PaperOrderRoleId { get; set; }

		public string Name { get; set; }

		public HashSet<OptCounterparty> CounterParties { get; set; }
	}
}
