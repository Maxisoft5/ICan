using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface IWarehouseRepository
	{
		Task<int> CreateAsync(OptWarehouse warehouse);
		Task CreateItemAsync(OptWarehouseItem rawItem);
		Task DeleteAsync(int warehouseId);

		Task<OptWarehouse> GetLatestWarehouseByTypeAsync(int whActionTypeId, int whObjectType, int? whType = null);
		WhJournalObjectType GetWhjObjectTypeByWh(WarehouseType warehouseTypeId);
		Task<IEnumerable<OptWarehouse>> GetWarehousesByWhType(int whTypeId);
		IQueryable<OptWarehouse> GetWarehousesByWhTypeAndActionType(WarehouseType whTypeId, WarehouseActionType whActionType);
		IQueryable<OptWarehouseType> GetWarehousesTypesByWhObjectType(WhJournalObjectType whObjectType);
		OptWarehouse GetLatestInventory(WarehouseType whTypeId);
		Task<OptWarehouse> GetAsync(int warehouseId);
		Task<OptWarehouseType> GetWareHouseTypeByType(WarehouseType warehouseType);
		IQueryable<OptWarehouse> GetWarehouseByType(int whActionTypeId, int whObjectType, int? whType = null);
	}
}