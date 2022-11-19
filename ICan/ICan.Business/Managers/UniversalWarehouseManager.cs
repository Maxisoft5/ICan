using AutoMapper;
using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class UniversalWarehouseManager : BaseManager
	{
		private readonly WarehouseJournalManager _whjManager;
		private readonly ProductManager _productManager;
		private readonly SemiproductManager _semiproductManager;
		private readonly SpringManager _springManager;
		private readonly IWarehouseRepository _whRepository;
		public UniversalWarehouseManager(
			IMapper mapper,
			ILogger<BaseManager> logger,
			WarehouseJournalManager whjManager,
			ProductManager productManager,
			SemiproductManager semiproductManager,
			SpringManager springManager,
			IWarehouseRepository whRepository)
			: base(mapper, logger)
		{
			_whRepository = whRepository;
			_whjManager = whjManager;
			_productManager = productManager;
			_semiproductManager = semiproductManager;
			_springManager = springManager;
		}

		public async Task<UniversalWarehouseModel> GetWarehouseInfo(WarehouseType whType, WhJournalObjectType whObjectType, IEnumerable<long> objectIds)
		{
			var warehouseRaws = await _whRepository.GetWarehousesByWhType((int)whType);
			var lastInventory = warehouseRaws.Where(x => x.WarehouseActionTypeId == (int)WarehouseActionType.Inventory
								&& x.WarehouseType.WarehouseObjectType == (int)whObjectType
								&& x.WarehouseTypeId == (int)whType).FirstOrDefault();

			var whJournal = _whjManager.Get(lastInventory?.DateAdd, objectIds, whObjectType,
				whType);
			return new UniversalWarehouseModel
			{
				Warehouses = _mapper.Map<IEnumerable<WarehouseModel>>(warehouseRaws),
				WhJournal = whJournal,
				LastInventoryAmount = lastInventory?.WarehouseItems.FirstOrDefault().Amount,
				LastInventoryDate = lastInventory != null ? lastInventory.DateAdd : null
			};
		}

		public async Task Delete(int id)
		{
			await _whRepository.DeleteAsync(id);
		}

		public async Task<WarehouseModel> GetDetails(int id, WhJournalObjectType whObjectType)
		{
			var warehouse = await _whRepository.GetAsync(id);
			var model = _mapper.Map<WarehouseModel>(warehouse);
			await SetObjectDisplayName(model.WarehouseItems, whObjectType);

			return model;
		}

		public async Task<IEnumerable<CalcUniversalWhjDetails>> GetState(WarehouseType whType)
		{
			var inventory = _whRepository.GetLatestInventory(whType);
			var journal = _whjManager.Get(inventory?.DateAdd, whType);

			var items = (await GetItems(whType)).ToList();
			foreach (var item in items)
			{
				item.Journal = journal.Where(j => j.ObjectId == item.ObjectId);
				item.InventoryDate = inventory?.DateAdd;
				item.InventoryAmount = inventory?.WarehouseItems?.FirstOrDefault(whI => whI.ObjectId == item.ObjectId)?.Amount ?? 0;
			}
			return items;
		}

		private async Task<IEnumerable<CalcUniversalWhjDetails>> GetItems(WarehouseType whType)
		{
			var whObjectType = _whRepository.GetWhjObjectTypeByWh(whType);

			Func<int, string> getName = (id) => string.Empty;
			switch (whObjectType)
			{
				case WhJournalObjectType.Semiproduct:
					{
						var semiproducts = await _semiproductManager.GetSemiproductList();
						return semiproducts.Select(sm => new CalcUniversalWhjDetails { ObjectDisplayName = sm.DisplayName, ObjectId = sm.SemiproductId });
					};

				case WhJournalObjectType.Spring:
					{
						var springs = await _springManager.GetSprings();
						return springs.Select(sm => new CalcUniversalWhjDetails { ObjectDisplayName = sm.SpringName, ObjectId = sm.SpringId });
					}
				case WhJournalObjectType.GluePad:
					{
						return new List<CalcUniversalWhjDetails> { new CalcUniversalWhjDetails {
						ObjectId = Const.GluePadProductId, ObjectDisplayName = "Клеевые подушки" }};
					}
				default:
					{
						var products = await _productManager.GetProductsByType("all");
						return products.Select(sm => new CalcUniversalWhjDetails { ObjectDisplayName = sm.DisplayName, ObjectId = sm.ProductId });
					}
			}
		}

		private async Task SetObjectDisplayName(IEnumerable<WarehouseItemModel> items, WhJournalObjectType whObjectType)
		{
			Func<int, string> getName = (id) => string.Empty;
			switch (whObjectType)
			{
				case WhJournalObjectType.Semiproduct:
					{
						var semiproducts = await _semiproductManager.GetSemiproductList();
						getName = (id) => semiproducts.FirstOrDefault(sm => sm.SemiproductId == id)?.DisplayName ?? string.Empty;
						break;
					};

				case WhJournalObjectType.Spring:
					{
						var springs = await _springManager.GetSprings();
						getName = (id) => (springs.FirstOrDefault(sm => sm.SpringId == id)?.SpringName ?? string.Empty);
						break;
					}
				case WhJournalObjectType.GluePad:
					{
						getName = (id) => "Клеевые подушки";
						break;
					}
				default:
					{
						var products = await _productManager.GetProductsByType("all");
						getName = (id) =>
							products.FirstOrDefault(prod => prod.ProductId == id)
							?.DisplayName ?? string.Empty;
						break;
					}
			}

			foreach (var item in items)
			{
				item.ObjectDisplayName = getName(item.ObjectId.Value);
			}
		}

		public async Task AddInventory(WarehouseModel model)
		{
			var warehouse = new OptWarehouse
			{
				DateAdd = DateTime.Now,
				WarehouseActionTypeId = (int)WarehouseActionType.Inventory,
				WarehouseTypeId = (int)model.WarehouseTypeId,
				Comment = model.Comment,
				WarehouseItems = model.WarehouseItems.Select(item =>
					new OptWarehouseItem
					{
						Amount = item.Amount,
						ObjectId = item.ObjectId,
					}).ToList()
			};

			await _whRepository.CreateAsync(warehouse);
		}
	}
}
