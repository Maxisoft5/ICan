using ICan.Common.Domain;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class CalcListsInfo
	{
		public WarehouseModel Initial { get; set; }
		public IEnumerable<OptOrderproduct> OrderItems { get; set; } = Enumerable.Empty<OptOrderproduct>();
		public IEnumerable<OptWarehouse> WareHouse { get; set; } = Enumerable.Empty<OptWarehouse>();
		public IEnumerable<OptReport> Upd { get; set; } = Enumerable.Empty<OptReport>();
		public IDictionary<int, int> MainProducts { get; set; } = ImmutableDictionary<int, int>.Empty;
	}
}