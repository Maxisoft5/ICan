using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface IWarehouseJournalRepository
	{
		Task<long> Add(OptWarehouseJournal whJournal, bool needSave = true);
		Task AddRange(IEnumerable<OptWarehouseJournal> whJournals, bool needSave = true);
		Task RemoveRange(IEnumerable<OptWarehouseJournal> whJournals, bool needSave = true);
		IEnumerable<OptWarehouseJournal> GetByAction(string actionId, int actionExtendedType);
		IEnumerable<OptWarehouseJournal> GetByParams(DateTime? date, IEnumerable<long> objectIds, WhJournalObjectType whJournalObjectType, WarehouseType whType);
		IEnumerable<OptWarehouseJournal> GetByActionIds(IEnumerable<string> actionIds);
		Task UpdateRange(IEnumerable<OptWarehouseJournal> whJournalRaws, bool needSave = true);
		Task Update(OptWarehouseJournal whJournal, bool needSave = true);
		Task RemoveRangeByAction(string actionId, int actionExtendedType, bool needSave = true);
		IEnumerable<OptWarehouseJournal> GetByParams(DateTime? date, WarehouseType whType);
	}
}
