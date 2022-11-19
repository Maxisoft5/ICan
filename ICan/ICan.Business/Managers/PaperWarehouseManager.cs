using AutoMapper;
using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class PaperWarehouseManager : WarehouseManager
	{
		public PaperWarehouseManager(IMapper mapper, IWarehouseRepository warehouseRepository, IProductRepository productRepository, ApplicationDbContext context, ILogger<BaseManager> logger,
			  ProductManager productManager,
			   WarehouseJournalManager whJournalManager,
			   SemiproductWarehouseManager semiproductWarehouseManager,
			 CalcManager calcManager,
			 CommonManager<OptPaper> paperManager) : base(mapper, warehouseRepository, productRepository, context, logger, productManager, whJournalManager, semiproductWarehouseManager,
				 calcManager, paperManager)
		{
			_excludedActions = new List<int> {
				(int)WarehouseActionType.Correction,
				(int)WarehouseActionType.Snapshot,
				(int)WarehouseActionType.Arrival,
				(int)WarehouseActionType.KitAssembly,
				(int)WarehouseActionType.Marketing,
				(int)WarehouseActionType.Returning,
			};
		}

		protected override async Task AddWarehouseItemsAsync(WarehouseModel model, int warehouseId, List<WarehouseJournalModel> journal, WarehouseModel inventory)
		{
			foreach (var item in model.WarehouseItems)
			{
				if (item.Amount > 0)
				{
					var whItem = new OptWarehouseItem
					{
						Amount = item.Amount,
						ObjectId = item.ObjectId,
						WarehouseId = warehouseId
					};
					await _context.AddAsync(whItem);
					await _context.SaveChangesAsync();
				}
			}
		}

		public override async Task SetListsAsync(WarehouseModel model)
		{
			model.WarehouseActionTypes = _calcManager.GetActions(excludedActions: _excludedActions);
			var paperModels = await _paperManager.GetAsync();
			model.PaperItems = _mapper.Map<IEnumerable<PaperWarehouseModel>>(paperModels);
			model.WarehouseTypes = GetWarehouseTypesList();
		}

		public override async Task<WarehouseModel> GetDetailsModelAsync(OptWarehouse raw, ActionType actionType)
		{
			var model = _mapper.Map<WarehouseModel>(raw);
			if (model != null)
			{
				model.PaperItems = await GetPaperItemsByWarehouseId(model.WarehouseId);
				model.WarehouseActionTypes = _calcManager.GetActions(excludedActions: _excludedActions);
				model.WarehouseTypes = GetWarehouseTypesList();
			}
			return model;
		}

		public override async Task Update(WarehouseModel model)
		{
			var warehouse = _context.OptWarehouse
									.Include(wh => wh.WarehouseItems)
									.First(wh => wh.WarehouseId == model.WarehouseId);
			warehouse.DateAdd = model.DateAdd;
			warehouse.WarehouseTypeId = (int)model.WarehouseTypeId;
			warehouse.Comment = model.Comment;

			foreach (var item in model.WarehouseItems)
			{
				var whItem = warehouse.WarehouseItems.FirstOrDefault(x => x.ObjectId == item.ObjectId);
				if (whItem != null)
				{
					whItem.Amount = item.Amount;
				}
				else
				{
					var newItem = new OptWarehouseItem
					{
						ObjectId = item.ObjectId,
						Amount = item.Amount,
						Warehouse = warehouse
					};
					await _context.AddAsync(newItem);
				}
			}
			_context.Update(warehouse);

			await _context.SaveChangesAsync();
		}

		public IEnumerable<SelectListItem> GetWarehouseTypesList()
		{
			var warehouseTypes = GetWarehouseTypes();
			var selectList = warehouseTypes
				.Select(type => new SelectListItem { Value = type.WarehouseTypeId.ToString(), Text = type.Name });
			return selectList;
		}

		public IEnumerable<WarehouseTypeModel> GetWarehouseTypes()
		{
			var warehouseTypes = _context.OptWarehouseType.Where(x => x.WarehouseObjectType == (int)WhJournalObjectType.Paper && x.ReadyToUse).Include(x => x.Counterparty);
			return _mapper.Map<IEnumerable<WarehouseTypeModel>>(warehouseTypes);
		}

		protected override async Task<string> GetSingleInventoryCommentAsync(WarehouseModel model)
		{
			var changedProduct = model.WarehouseItems.First(item => item.Amount > 0);
			var paper = await _paperManager.GetAsync(changedProduct.ObjectId);
			var state = await _calcManager.CalculatePaperFromWhjAsync(model.WarehouseTypeId);
			var oldValue = state.First(stateItem => stateItem.PaperId == changedProduct.ObjectId).Current;
			return $"Изменено значение для {paper.Name}, было {oldValue}, стало {changedProduct.Amount}";

		}

		private async Task<IEnumerable<PaperWarehouseModel>> GetPaperItemsByWarehouseId(int warehouseId)
		{
			var warehouse = await _context.OptWarehouse.Include(x => x.WarehouseItems).FirstAsync(x => x.WarehouseId == warehouseId);
			var whItemObjectIds = warehouse.WarehouseItems.Select(x => x.ObjectId).ToList();
			var paper = _mapper.Map<List<PaperWarehouseModel>>(_context.OptPaper);

			paper.ForEach(x =>
			{
				if (whItemObjectIds.Contains(x.PaperId))
				{
					x.Amount = warehouse.WarehouseItems.First(f => f.ObjectId == x.PaperId).Amount;
				}
				else
				{
					x.Amount = 0;
				}
			});

			if ((int)WarehouseActionType.SingleInventory == warehouse.WarehouseActionTypeId)
			{
				return paper.Where(x => whItemObjectIds.Contains(x.PaperId));
			}

			return paper;
		}
	}
}
