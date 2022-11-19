using ICan.Common.Models.Enums;
using ICan.Data.Context;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Jobs.ClearWhJournal
{
	public class ClearWhJournalJob
	{
		private readonly ApplicationDbContext _context;

		public ClearWhJournalJob(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task ClearWhJournal(WhJournalObjectType objectToClear)
		{
			var whType = _context.OptWarehouseType.First(wh => wh.WarehouseObjectType ==
				(int)objectToClear && wh.ReadyToUse == true).WarehouseTypeId;
			var lastInventory = _context.OptWarehouse
										.Where(x => x.WarehouseActionTypeId == (int)WarehouseActionType.Inventory && x.WarehouseTypeId == whType)
										.OrderByDescending(x => x.DateAdd)
										.Skip(1).FirstOrDefault();

			if (lastInventory != null)
			{
				var lastInventoryDate = lastInventory.DateAdd;
				var whJournals = _context.OptWarehouseJournal
								.Where(x => x.ActionDate < lastInventoryDate && x.ObjectTypeId == (int)objectToClear);
				_context.RemoveRange(whJournals);
				await _context.SaveChangesAsync();
			}
		}
	}
}
