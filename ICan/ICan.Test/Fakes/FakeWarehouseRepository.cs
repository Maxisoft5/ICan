using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Test.Fakes
{
	public class FakeWarehouseRepository : IWarehouseRepository
	{
		private List<OptWarehouse> WarehouseEntries = new List<OptWarehouse>();

		private static readonly List<OptWarehouseType> _whTypes = new List<OptWarehouseType> {
			new OptWarehouseType {
				WarehouseTypeId =(int)WarehouseType.NotebookReady,
				WarehouseObjectType = (int)WhJournalObjectType.Notebook
			}, new OptWarehouseType {
				WarehouseTypeId = (int)WarehouseType.GluePads,
				WarehouseObjectType = (int)WhJournalObjectType.GluePad,
			}
		};


		public async Task<int> CreateAsync(OptWarehouse warehouse)
		{
			warehouse.WarehouseType = _whTypes
				.FirstOrDefault(wh => wh.WarehouseTypeId == warehouse.WarehouseTypeId);
			WarehouseEntries.Add(warehouse);
			return warehouse.WarehouseId;
		}

		public async Task CreateItemAsync(OptWarehouseItem rawItem)
		{
			var warehouse = await GetAsync(rawItem.WarehouseId);
			warehouse.WarehouseItems.Add(rawItem);
		}
		public async Task<OptWarehouse> GetAsync(int warehouseId)
		{
			var warehouse = WarehouseEntries.FirstOrDefault(wh => wh.WarehouseId == warehouseId);
			return warehouse;
		}

		public async Task DeleteAsync(int warehouseId)
		{
			var warehouse = await GetAsync(warehouseId);
			WarehouseEntries.Remove(warehouse);
		}

		public async Task DeleteItemsAsync(int warehouseId)
		{
			var warehouse = await GetAsync(warehouseId);
			warehouse.WarehouseItems = new HashSet<OptWarehouseItem>();
		}

		public async Task<OptWarehouse> GetLatestWarehouseByTypeAsync(int whActionTypeId, int whObjectType, int? whType = null)
		{
			return WarehouseEntries
				.Where(warehouse =>
					(whType != null ? (warehouse.WarehouseTypeId == whType) : true)
					&& warehouse.WarehouseActionTypeId == whActionTypeId
					&& warehouse.WarehouseType.WarehouseObjectType == whObjectType)
				.OrderByDescending(warehouse => warehouse.DateAdd)
				.FirstOrDefault();
		}

		public WhJournalObjectType GetWhjObjectTypeByWh(WarehouseType warehouseTypeId)
		{
			return WhJournalObjectType.Notebook;
		}

		public async Task<IEnumerable<OptWarehouse>> GetWarehousesByWhType(int whActionTypeId)
		{
			return WarehouseEntries.Where(x => x.WarehouseTypeId == whActionTypeId)
										.OrderByDescending(x => x.DateAdd);
		}

		public IQueryable<OptWarehouse> GetWarehousesByWhTypeAndActionType(WarehouseType whTypeId, WarehouseActionType whActionType)
		{
			return WarehouseEntries
				 .Where(wh => wh.WarehouseTypeId == (int)whTypeId
				 && wh.WarehouseActionTypeId == (int)whActionType).AsQueryable();
		}

		public IQueryable<OptWarehouseType> GetWarehousesTypesByWhObjectType(WhJournalObjectType whObjectType)
		{
			return _whTypes.Where(wh => wh.WarehouseObjectType == (int)whObjectType).AsQueryable();
		}

		public OptWarehouse GetLatestInventory(WarehouseType whTypeId)
		{
			var inventory = GetWarehousesByWhTypeAndActionType(whTypeId, WarehouseActionType.Inventory)
			.OrderByDescending(x => x.DateAdd)
			.FirstOrDefault();
			return inventory;
		}

		public IQueryable<OptWarehouse> GetWarehouseByType(int whActionTypeId, int whObjectType, int? whType = null)
		{
			return WarehouseEntries
				.Where(warehouse =>
					 (whType == null || warehouse.WarehouseTypeId == whType)
					&& warehouse.WarehouseActionTypeId == whActionTypeId
					&& warehouse.WarehouseType.WarehouseObjectType == whObjectType).AsQueryable();
		}

        public Task<OptWarehouseType> GetWareHouseTypeByType(WarehouseType warehouseType)
        {
            throw new System.NotImplementedException();
        }
    }
}
