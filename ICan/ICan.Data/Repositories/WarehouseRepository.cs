using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Data.Repositories
{
	public class WarehouseRepository : BaseRepository, IWarehouseRepository
	{
		public WarehouseRepository(ApplicationDbContext context) : base(context)
		{
		}

		public async Task<int> CreateAsync(OptWarehouse warehouse)
		{
			await _context.AddAsync(warehouse);
			await _context.SaveChangesAsync();
			return warehouse.WarehouseId;
		}

		public async Task CreateItemAsync(OptWarehouseItem rawItem)
		{
			await _context.AddAsync(rawItem);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(int warehouseId)
		{
			var warehouse = await GetAsync(warehouseId);
			if (warehouse == null)
				return;
			_context.Remove(warehouse);
			await _context.SaveChangesAsync();
		}

		public async Task<OptWarehouse> GetAsync(int warehouseId)
		{
			var warehouse = await _context.OptWarehouse
				.Include(wh => wh.WarehouseActionType)
				.Include(wh => wh.WarehouseItems)
				.FirstOrDefaultAsync(wh => wh.WarehouseId == warehouseId);
			return warehouse;
		}

		public async Task DeleteItemsAsync(int warehouseId)
		{
			var items = _context.OptWarehouseItem
				.Where(wh => wh.WarehouseId == warehouseId);
			if (items != null && items.Any())
			{
				_context.RemoveRange(items);
				await _context.SaveChangesAsync();
			}
		}

		public IQueryable<OptWarehouse> GetWarehouseByType(int whActionTypeId, int whObjectType, int? whType = null)
		{
			return _context.OptWarehouse
					.Include(t => t.WarehouseItems)
					.Include(warehouse => warehouse.WarehouseType)
					.Where(warehouse =>
						(whType == null  || warehouse.WarehouseTypeId == whType) 
						&& warehouse.WarehouseActionTypeId == whActionTypeId
						&& warehouse.WarehouseType.WarehouseObjectType == whObjectType);
		}

		public async Task<OptWarehouse> GetLatestWarehouseByTypeAsync(int whActionTypeId,
			int whObjectType, int? whType = null)
		{
			return await GetWarehouseByType(whActionTypeId, whObjectType, whType)
			.OrderByDescending(warehouse => warehouse.DateAdd)
				.FirstOrDefaultAsync();
		}

		public async Task<IEnumerable<OptWarehouse>> GetWarehousesByWhType(int whTypeId)
		{
			return await _context.OptWarehouse.Where(x => x.WarehouseTypeId == whTypeId)
										.Include(x => x.WarehouseItems)
										.Include(x => x.WarehouseActionType)
										.Include(x => x.WarehouseType)
										.OrderByDescending(x => x.DateAdd)
										.ToListAsync();
		}

		public WhJournalObjectType GetWhjObjectTypeByWh(WarehouseType warehouseTypeId)
		{
			var whType = _context.OptWarehouseType.First(warehouseT => warehouseT.WarehouseTypeId == (int)warehouseTypeId);
			return (WhJournalObjectType)whType.WarehouseObjectType;
		}

		public IQueryable<OptWarehouse> GetWarehousesByWhTypeAndActionType(WarehouseType whTypeId, WarehouseActionType whActionType)
		{
			return _context.OptWarehouse
			 .Include(x => x.WarehouseActionType)
			 .Include(x => x.WarehouseType)
			 .Include(wh => wh.WarehouseItems)
			 .Where(wh => wh.WarehouseTypeId == (int)whTypeId
			 && wh.WarehouseActionTypeId == (int)whActionType);
		}

		public OptWarehouse GetLatestInventory(WarehouseType whTypeId)
		{
			var inventory = GetWarehousesByWhTypeAndActionType(whTypeId, WarehouseActionType.Inventory)
						.OrderByDescending(x => x.DateAdd)
						.FirstOrDefault();
			return inventory;
		}


		public IQueryable<OptWarehouseType> GetWarehousesTypesByWhObjectType(WhJournalObjectType whObjectType)
		{
			return _context.OptWarehouseType
			 .Where(wh => wh.WarehouseObjectType == (int)whObjectType);
		}

    public async Task<OptWarehouseType> GetWareHouseTypeByType(WarehouseType warehouseType)
    {
      return await _context.OptWarehouseType.FirstOrDefaultAsync(x => x.WarehouseTypeId == (int)warehouseType);
    }
  }
}

