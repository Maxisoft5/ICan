using System.Collections.Generic;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class UpdCheckData
	{
		public readonly ILookup<string, UpdPaymentCheckModel> GrouppedUpds;

		public readonly IEnumerable<string> Months;

		public readonly IEnumerable<UpdPaymentModel> Unbound;

		public UpdCheckData(ILookup<string, UpdPaymentCheckModel>  grouppedUpds,
			IEnumerable<UpdPaymentModel> unbound
			) {
			GrouppedUpds = grouppedUpds;
			var keys = GrouppedUpds.Select(group => group.Key);

			Months = keys.SelectMany(key => GrouppedUpds[key])
				.OrderBy(upd => upd.ExpectingPaymentDate)
				.Select(upd => upd.ExpectingPaymentMontYear)
				.Distinct();

			Unbound = unbound;
		}
	}
}