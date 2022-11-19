using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Repositories;
using ICan.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Data.Repositories
{
	public class WarehouseJournalRepository : BaseRepository, IWarehouseJournalRepository
	{
		public WarehouseJournalRepository(ApplicationDbContext context) : base(context) { }

		public async Task<long> Add(OptWarehouseJournal whJournal, bool needSave = true)
		{
			await _context.AddAsync(whJournal);

			if (needSave == false)
				return default;

			await _context.SaveChangesAsync();
			return whJournal.WarehousejournalId;
		}

		public async Task AddRange(IEnumerable<OptWarehouseJournal> whJournals, bool needSave = true)
		{
			await _context.AddRangeAsync(whJournals);

			if (needSave == true)
				await _context.SaveChangesAsync();
		}

		public IEnumerable<OptWarehouseJournal> GetByAction(string actionId, int actionExtendedType)
		{
			return _context.OptWarehouseJournal
				.Where(whj => whj.ActionId.Equals(actionId)
				&& whj.ActionExtendedTypeId == actionExtendedType);
		}

		public IEnumerable<OptWarehouseJournal> GetByActionIds(IEnumerable<string> actionIds)
		{
			return _context.OptWarehouseJournal.Where(x => actionIds.Contains(x.ActionId));
		}

		public IEnumerable<OptWarehouseJournal> GetByParams(DateTime? date, IEnumerable<long> objectIds, WhJournalObjectType whJournalObjectType, WarehouseType whType)
		{
			return _context.OptWarehouseJournal
					.Where(whj => whj.ObjectTypeId == (int)whJournalObjectType
					&& objectIds.Contains(whj.ObjectId)
					&& whj.WarehouseTypeId == (int)whType
					&& (date != null ? whj.ActionDate >= date : true));
		}

		public IEnumerable<OptWarehouseJournal> GetByParams(DateTime? date, WarehouseType whType)
		{
			return _context.OptWarehouseJournal
					.Where(whj =>
					  whj.WarehouseTypeId == (int)whType
					&& (date != null ? whj.ActionDate >= date : true));
		}

		public async Task RemoveRange(IEnumerable<OptWarehouseJournal> whJournals, bool needSave = true)
		{
			_context.RemoveRange(whJournals);

			if (needSave == true)
				await _context.SaveChangesAsync();
		}

		public async Task UpdateRange(IEnumerable<OptWarehouseJournal> whJournalRaws, bool needSave = true)
		{
			_context.UpdateRange(whJournalRaws);

			if (needSave == true)
				await _context.SaveChangesAsync();
		}

		public async Task Update(OptWarehouseJournal whJournal, bool needSave = true)
		{
			_context.Update(whJournal);

			if (needSave == true)
				await _context.SaveChangesAsync();
		}

		public async Task RemoveRangeByAction(string actionId, int actionExtendedType, bool needSave = true)
		{
			var rawList = _context.OptWarehouseJournal.Where(whj =>
						whj.ActionId.Equals(actionId)
						&& whj.ActionExtendedTypeId == actionExtendedType);
			_context.RemoveRange(rawList);

			if (needSave == true)
				await _context.SaveChangesAsync();
		}
	}
}
