using AutoMapper;
using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class SemiproductWarehouseManager : BaseManager
	{
		private readonly SemiproductManager _semiproductManager;
		private readonly ProductManager _productManager;
		private readonly WarehouseJournalManager _whJournalManager;
		private readonly CalcManager _calcManager;
		private readonly IWarehouseRepository _warehouseRepository;
		private readonly static Color _noSemiproductColor = ColorTranslator.FromHtml("#df7e2d");
		public SemiproductWarehouseManager(IMapper mapper,
			ApplicationDbContext context,
			ILogger<BaseManager> logger,
			ProductManager productManager,
			WarehouseJournalManager whJournalManager,
			CalcManager calcManager,
			SemiproductManager semiproductManager,
			IWarehouseRepository warehouseRepository
			) : base(mapper, context, logger)
		{
			_productManager = productManager;
			_calcManager = calcManager;
			_whJournalManager = whJournalManager;
			_semiproductManager = semiproductManager;
			_warehouseRepository = warehouseRepository;
		}

		public IEnumerable<WarehouseModel> GetWarehouseList()
		{
			var whTypes = _warehouseRepository.GetWarehousesTypesByWhObjectType(WhJournalObjectType.Semiproduct).Select(wh => wh.WarehouseTypeId).ToList();
			var raw = _context.OptWarehouse
				.Include(wh => wh.WarehouseType)
				.Include(wh => wh.WarehouseActionType)
				.Where(wh => (wh.WarehouseActionTypeId == (int)WarehouseActionType.Inventory
				|| wh.WarehouseActionTypeId == (int)WarehouseActionType.SingleInventory) && whTypes.Contains(wh.WarehouseTypeId))
				.Include(semiWrhs => semiWrhs.WarehouseActionType)
				.OrderByDescending(semiWhrs => semiWhrs.DateAdd);
			var list = _mapper.Map<IEnumerable<WarehouseModel>>(raw);
			return list;
		}

		public SemiproductWarehouseModel GetWarehouse(int? id)
		{
			var raw = _context.OptWarehouse
						.Include(semiWrhs => semiWrhs.WarehouseItems)
						.Include(semiWrhs => semiWrhs.WarehouseActionType)
						.Include(semiWrhs => semiWrhs.WarehouseType)
						.First(semiWrhs => semiWrhs.WarehouseId == id.Value);

			var warehouseItems = raw
				.WarehouseItems.ToList();

			var model = new SemiproductWarehouseModel
			{
				Comment = raw.Comment,
				Date = raw.DateAdd,
				SemiproductWarehouseId = raw.WarehouseId,
				WarehouseActionTypeId = raw.WarehouseActionTypeId,
				WarehouseActionTypeName = raw.WarehouseActionType.Name,
				WarehouseTypeId = raw.WarehouseTypeId,
				WarehouseTypeName = raw.WarehouseType.Name
			};
			model.SemiproductWarehouseFullItems = GetSemiproductsFullList()
				.Select(item => SetSemiproductAmount(item, warehouseItems));

			return model;
		}

		public async Task Update(SemiproductWarehouseModel model)
		{
			var raw = _context.OptWarehouse.Find(model.SemiproductWarehouseId);
			var rawItems = _context.OptWarehouseItem.Where(smwhItem => smwhItem.WarehouseId == model.SemiproductWarehouseId);
			foreach (var item in model.SemiproductWarehouseFullItems)
			{
				UpdateIfNotEmpty(item.BlockId, item.BlockAmount, rawItems, raw);
				UpdateIfNotEmpty(item.StickersId, item.StickersAmount, rawItems, raw);
				UpdateIfNotEmpty(item.CoversId, item.CoversAmount, rawItems, raw);
				UpdateIfNotEmpty(item.BoxFrontId, item.BoxFrontAmount, rawItems, raw);
				UpdateIfNotEmpty(item.BoxBackId, item.BoxBackAmount, rawItems, raw);
				UpdateIfNotEmpty(item.CursorId, item.CursorAmount, rawItems, raw);
			}

			await _context.SaveChangesAsync();
		}

		public async Task<KeyValuePair<string, IEnumerable<SemiproductCalcDetails>>> CalcSemiproductWhjournalAsync(int productId,
			OptWarehouse lastInventoryReady = null,
			IEnumerable<OptWarehouseJournal> whJournalsBySemiproductIds = null, IEnumerable<OptWarehouse> allPartialInventories = null,
			List<OptPrintOrder> allNotAssembledPrintOrders = null, Stopwatch sw = null)
		{
			var product = _productManager.GetDetails(productId);
			lastInventoryReady = lastInventoryReady ?? _warehouseRepository.GetLatestInventory(WarehouseType.SemiproductReady);
			var semiProdsDetails = new List<SemiproductCalcDetails>();
			var semiproductsByProductId = GetSemiproductsByProductId(productId).ToList();
			var semiproductIds = semiproductsByProductId.Select(x => x.SemiproductId);

			OptWarehouse lastUnNotchedInventory = _warehouseRepository.GetLatestInventory(WarehouseType.StickersUnNotched);
			OptWarehouse lastNotchingInventory = _warehouseRepository.GetLatestInventory(WarehouseType.StickersNotching);
			var minDate = GetMinDate(lastInventoryReady, lastUnNotchedInventory, lastNotchingInventory);
			whJournalsBySemiproductIds = GetAllJournal(minDate).Where(whj => semiproductIds.Contains(whj.ObjectId));
			
			allPartialInventories = allPartialInventories ??
					   _warehouseRepository.GetWarehouseByType((int)WarehouseActionType.SingleInventory, (int)WhJournalObjectType.Semiproduct) 
					   .ToList();

			var partialInventoriesReadyWh = allPartialInventories.Where(wh =>
						wh.WarehouseTypeId == (int)WarehouseType.SemiproductReady &&
						wh.WarehouseItems.Any(item => item.ObjectId.HasValue && semiproductIds.Contains(item.ObjectId.Value))
						&& wh.DateAdd >= lastInventoryReady.DateAdd);

			var partialInventoriesUnNotched = allPartialInventories.Where(wh =>
						(lastUnNotchedInventory == null || wh.DateAdd >= lastUnNotchedInventory.DateAdd) &&
						wh.WarehouseTypeId == (int)WarehouseType.StickersUnNotched &&
						wh.WarehouseItems.Any(item => item.ObjectId.HasValue && semiproductIds.Contains(item.ObjectId.Value)));

			var partialInventoriesNotching = allPartialInventories.Where(wh =>
						wh.WarehouseTypeId == (int)WarehouseType.StickersNotching &&

						(lastNotchingInventory == null || wh.DateAdd >= lastNotchingInventory.DateAdd) &&
						wh.WarehouseItems.Any(item => item.ObjectId.HasValue && semiproductIds.Contains(item.ObjectId.Value)));

			var warehouseReadyState = whJournalsBySemiproductIds
				.Where(whj => whj.WarehouseTypeId == (int)WarehouseType.SemiproductReady);
			if (lastInventoryReady != null)
			{
				warehouseReadyState = warehouseReadyState.Where(whj => whj.ActionDate >= lastInventoryReady.DateAdd);
				partialInventoriesReadyWh = partialInventoriesReadyWh.Where(inv => inv.DateAdd >= lastInventoryReady.DateAdd);
				whJournalsBySemiproductIds = whJournalsBySemiproductIds.Where(whj => whj.ActionDate >= lastInventoryReady.DateAdd);
			}

			allNotAssembledPrintOrders = allNotAssembledPrintOrders ?? await _context.OptPrintOrder
				  .Include(printOrder => printOrder.PrintOrderSemiproducts)
					  .ThenInclude(printOrderSm => printOrderSm.SemiProduct)
				  .Include(printOrder => printOrder.PrintOrderIncomings)
					  .ThenInclude(printOrderInc => printOrderInc.PrintOrderIncomingItems)
			  .Where(printOrder => !(printOrder.IsAssembled ?? false)).ToListAsync();
			AddStopWatchLog(sw, "inside calc before cycle");
			foreach (var semiproduct in semiproductsByProductId.OrderBy(sm => sm.SemiproductTypeId))
			{
				var partialInventory = partialInventoriesReadyWh
					.Where(inv => inv.WarehouseItems.Any(item => item.ObjectId == semiproduct.SemiproductId))
					.OrderByDescending(inv => inv.DateAdd).FirstOrDefault();
				var partialInventoryItem = partialInventory?.WarehouseItems.First(item => item.ObjectId == semiproduct.SemiproductId);

				var semiproductModel = _mapper.Map<SemiproductModel>(semiproduct);
				var semiproductWhJournal = whJournalsBySemiproductIds.Where(whj => whj.ObjectId == semiproduct.SemiproductId
						&& (partialInventory == null || whj.ActionDate >= partialInventory.DateAdd)).ToList();

				var semiproductWhJournalReady = warehouseReadyState.Where(x => x.ObjectId == semiproduct.SemiproductId
				&& (partialInventory == null || x.ActionDate >= partialInventory.DateAdd)).ToList();

				// printorders
				var printorderItems = semiproductWhJournal.Where(whj =>
					(whj.WarehouseTypeId == (int)WarehouseType.SemiproductReady &&
					semiproduct.SemiproductTypeId != (int)SemiProductType.Stickers
					|| whj.WarehouseTypeId == (int)WarehouseType.StickersUnNotched && semiproduct.SemiproductTypeId == (int)SemiProductType.Stickers)
					&& whj.ObjectTypeId == (int)WhJournalObjectType.Semiproduct
					&& whj.ActionExtendedTypeId == (int)WhJournalActionExtendedType.PrintOrder);

				// stickers un notched
				var unnotchedJournal = whJournalsBySemiproductIds.Where(wh => wh.WarehouseTypeId == (int)WarehouseType.StickersUnNotched && wh.ObjectId == semiproduct.SemiproductId);
				var baseLine = 0;
				if (lastUnNotchedInventory != null && semiproduct.SemiproductTypeId == (int)SemiProductType.Stickers)
				{
					unnotchedJournal = unnotchedJournal.Where(wh => wh.ActionDate >= lastUnNotchedInventory.DateAdd);
					baseLine = lastUnNotchedInventory.WarehouseItems.FirstOrDefault(item => item.ObjectId == semiproduct.SemiproductId)?.Amount ?? 0;
				}
				var currentPartialInventoriesUnNotched = partialInventoriesUnNotched.Where(inv => inv.WarehouseItems.Any(item => item.ObjectId == semiproduct.SemiproductId)).OrderByDescending(inv => inv.DateAdd).FirstOrDefault();
				if (currentPartialInventoriesUnNotched != null && semiproduct.SemiproductTypeId == (int)SemiProductType.Stickers)
				{
					unnotchedJournal = unnotchedJournal.Where(wh => wh.ActionDate >= currentPartialInventoriesUnNotched.DateAdd);
					baseLine = currentPartialInventoriesUnNotched.WarehouseItems.FirstOrDefault(item => item.ObjectId == semiproduct.SemiproductId)?.Amount ?? 0;
				}
				int stickersUnNotched = baseLine + WarehouseJournalManager.GetSubState(unnotchedJournal, (int)WarehouseType.StickersUnNotched);
				baseLine = 0;
				var notchingJournal = whJournalsBySemiproductIds.Where(wh => wh.WarehouseTypeId == (int)WarehouseType.StickersNotching && wh.ObjectId == semiproduct.SemiproductId);
				if (lastNotchingInventory != null)
				{
					notchingJournal = notchingJournal.Where(wh => wh.ActionDate >= lastNotchingInventory.DateAdd);
					baseLine = lastNotchingInventory.WarehouseItems.FirstOrDefault(item => item.ObjectId == semiproduct.SemiproductId)?.Amount ?? 0;
				}
				var currentPartialInventoriesNotching = partialInventoriesNotching.Where(inv => inv.WarehouseItems.Any(item => item.ObjectId == semiproduct.SemiproductId)).OrderByDescending(inv => inv.DateAdd).FirstOrDefault();
				_logger.LogWarning($" calc {productId}");
				if (currentPartialInventoriesNotching != null)
				{
					notchingJournal = notchingJournal.Where(wh => wh.ActionDate >= currentPartialInventoriesNotching.DateAdd);
					baseLine = currentPartialInventoriesNotching.WarehouseItems.FirstOrDefault(item => item.ObjectId == semiproduct.SemiproductId)?.Amount ?? 0;
				}
				_logger.LogWarning($" success calc {productId}");
				// stickers notching

				var stickersNotching = baseLine + notchingJournal.Where(x => x.ActionTypeId == (int)WhJournalActionType.Income)
					.Sum(x => x.Amount)
					 - notchingJournal.Where(x => x.ActionTypeId == (int)WhJournalActionType.Outcome).Sum(x => x.Amount);
				// stickers ready 
				// no need in baseLine here 				
				var semiproductsReadyJournalSum = semiproductWhJournalReady
			   .Where(x => x.ActionTypeId == (int)WhJournalActionType.Income).Sum(x => x.Amount)
				- semiproductWhJournalReady.Where(x => x.ActionTypeId == (int)WhJournalActionType.Outcome).Sum(x => x.Amount);

				//assemly info
				var assemblyIdsWithSemiproduct = semiproductWhJournalReady
					.Where(whj => whj.ActionExtendedTypeId == (int)WhJournalActionExtendedType.AssemblyPart
						&& whj.WarehouseTypeId == (int)WarehouseType.SemiproductReady)
					.Select(x => long.Parse(x.ActionId));
				AddStopWatchLog(sw, "inside calc before assemblies");
				var assemblyItems = new List<OptAssembly>();
				var assemblies = _context.OptAssembly.Where(x => assemblyIdsWithSemiproduct.Contains(x.AssemblyId)).ToList();
				assemblyItems.AddRange(assemblies);

				//kitAssembly
				List<OptWarehouseItem> kitAssemblies = await GetKitAssemblies(semiproductWhJournalReady);
				AddStopWatchLog(sw, "inside calc after kitAssemblies");
				//printOrders info
				var printOrdersWithSemiproduct = allNotAssembledPrintOrders.Where(printOder => printOder.PrintOrderSemiproducts.Any(a => a.SemiproductId == semiproduct.SemiproductId));

				var listRestModels = new List<PrintOrderRestAmountModel>();
				AddStopWatchLog(sw, "inside calc before printingInProcess");
				var printingInProcess = _calcManager.CalcPrintingInOrders(semiproduct.SemiproductId, printOrdersWithSemiproduct, listRestModels);
				AddStopWatchLog(sw, "inside calc after printingInProcess");
				var semiproductWhItem = lastInventoryReady?.WarehouseItems.FirstOrDefault(x => x.ObjectId == semiproduct.SemiproductId);

				semiProdsDetails.Add(new SemiproductCalcDetails
				{
					ProductId = product.ProductId,
					SemiproductId = semiproduct.SemiproductId,
					SemiproductName = semiproductModel.Name,
					SemiproductTypeId = semiproductModel.SemiproductTypeId,
					SemiproductDisplayName = semiproductModel.DisplayName,
					Assemblies = assemblyItems,
					InventoryDate = lastInventoryReady?.DateAdd,
					Inventory = semiproductWhItem?.Amount ?? 0,
					SingleInventory = partialInventoryItem?.Amount,
					SingleInventoryDate = partialInventory?.DateAdd,
					PrintOrderIncomingItems = printorderItems,
					PrintOrders = printOrdersWithSemiproduct,
					PrintingInProcess = printingInProcess,
					PrintOrdersInProgress = listRestModels,
					KitAssemblies = _mapper.Map<IEnumerable<WarehouseItemModel>>(kitAssemblies),
					StickersUnNotchedSum = stickersUnNotched,
					StickersNotchingSum = stickersNotching,
					SemiproductsReadyJournalSum = semiproductsReadyJournalSum
				});
			}

			return new KeyValuePair<string, IEnumerable<SemiproductCalcDetails>>(product.DisplayName, semiProdsDetails);
		}

		private async Task<List<OptWarehouseItem>> GetKitAssemblies(List<OptWarehouseJournal> semiproductWhJournalReady)
		{
			var kitAssemblyIdsWithSemiproduct = semiproductWhJournalReady
								.Where(whj => whj.ActionExtendedTypeId == (int)WhJournalActionExtendedType.KitAssemblyPart
									&& whj.WarehouseTypeId == (int)WarehouseType.SemiproductReady)
								.Select(whj => long.Parse(whj.ActionId));
			var kitAssemblies = await _context.OptWarehouseItem
				.Include(x => x.Warehouse)
				.Where(x => kitAssemblyIdsWithSemiproduct.Contains(x.WarehouseId))
				.ToListAsync();
			return kitAssemblies;
		}

		public async Task<IEnumerable<SemiproductModel>> GetSemiproductsForInventory(int productSeriesId, int whType)
		{
			IEnumerable<OptSemiproduct> semiprods = await _context.OptSemiproduct
				.Include(x => x.Product)
					.ThenInclude(x => x.Country)
				.Include(x => x.Product)
					.ThenInclude(x => x.ProductSeries)
				.Include(x => x.SemiproductType)
				.Where(x => productSeriesId == 0 || x.Product.ProductSeriesId == productSeriesId).ToListAsync();
			if (whType != (int)WarehouseType.SemiproductReady)
			{
				semiprods = semiprods.Where(sm => sm.SemiproductTypeId == (int)SemiProductType.Stickers);
			}
			return _mapper.Map<IEnumerable<SemiproductModel>>(semiprods);
		}

		public IEnumerable<SemiproductModel> GetSemiprudctsForPartialInventory(int productId)
		{
			var semiproducts = _context.OptSemiproduct.Where(x => x.ProductId == productId)
				.Include(x => x.SemiproductType)
				.Include(x => x.Product)
				.ThenInclude(x => x.ProductSeries);
			var mappedSemiprods = _mapper.Map<IEnumerable<SemiproductModel>>(semiproducts);
			return mappedSemiprods;
		}

		public async Task AddPartial(PartialInventoryModel model)
		{
			var changedSemiproduct = await _semiproductManager.GetSemiproductAsync(model.SemiproductId);

			var currentSnapshotSemiproductWarehouse = await CalcSemiproductWhjournalAsync(changedSemiproduct.ProductId);
			var semiproductList = currentSnapshotSemiproductWarehouse.Value;
			var semiproductDetails = semiproductList.First(stateItem => stateItem.SemiproductId == model.SemiproductId);
			var oldValue = semiproductDetails.Current;
			if (model.WarehouseTypeId == (int)WarehouseType.StickersNotching)
			{
				oldValue = semiproductDetails.StickersNotchingSum;
			}
			else if (model.WarehouseTypeId == (int)WarehouseType.StickersUnNotched)
			{
				oldValue = semiproductDetails.StickersUnNotchedSum;
			}

			var comment = $"{changedSemiproduct.DisplayName}, было {oldValue}, стало { model.Amount}";
			await AddPartialToWarehouse(model, comment);

			await _context.SaveChangesAsync();
		}

		public IEnumerable<SemiproductWarehouseFullModel> GetSemiproductsFullList()
		{
			var products = _context.OptProduct
				.Include(prod => prod.Semiproducts)
					.ThenInclude(semiprod => semiprod.SemiproductType)
				.Include(prod => prod.ProductSeries)
				.OrderBy(prod => prod.ProductSeries.Order)
					.ThenBy(prod => prod.DisplayOrder)
				.Where(prod => !prod.IsArchived && prod.ProductKindId == 1)
				.ToList();

			var semiProductList = products.SelectMany(product => product.Semiproducts);
			var semiproducts = new List<SemiproductWarehouseFullModel>();
			foreach (var product in products)
			{
				var semiProductInfo = GetSemiproductIds(semiProductList, product.ProductId);

				semiproducts.Add(new SemiproductWarehouseFullModel
				{
					ProductId = product.ProductId,
					ProductName = product.Name,
					IsKit = product.IsKit,
					SeriesName = product.ProductSeries.Name,
					SeriesId = product.ProductSeries.ProductSeriesId,
					BlockId = semiProductInfo.BlockId,
					CoversId = semiProductInfo.CoversId,
					StickersId = semiProductInfo.StickersId,
					BoxFrontId = semiProductInfo.BoxFrontId,
					BoxBackId = semiProductInfo.BoxBackId,
					CursorId = semiProductInfo.CursorId,
					PointerId = semiProductInfo.PointerId
				});
			}
			return semiproducts;
		}

		public SemiproductContent GetSemiproductIds(IEnumerable<OptSemiproduct> semiProducts, int productId, bool canBeUniversal = false)
		{
			semiProducts = semiProducts ??
				_context
				.OptSemiproduct
				.Include(t => t.SemiproductType)
				.Include(t => t.Product)
				.Where(t => t.ProductId == productId);

			var semiproductContent = new SemiproductContent();
			semiproductContent.BlockId = semiProducts
			  .FirstOrDefault(sProduct => sProduct.ProductId == productId && sProduct.SemiproductTypeId == (int)SemiProductType.Block)?.SemiproductId;
			semiproductContent.StickersId = semiProducts
			  .FirstOrDefault(sProduct => (canBeUniversal || sProduct.ProductId == productId) && sProduct.SemiproductTypeId == (int)SemiProductType.Stickers)?.SemiproductId;
			semiproductContent.CoversId = semiProducts
			  .FirstOrDefault(sProduct => sProduct.ProductId == productId && sProduct.SemiproductTypeId == (int)SemiProductType.Covers)?.SemiproductId;

			semiproductContent.BoxFrontId = semiProducts
			  .FirstOrDefault(sProduct => sProduct.ProductId == productId && sProduct.SemiproductTypeId == (int)SemiProductType.Box && !string.IsNullOrWhiteSpace(sProduct.Name)
				&& sProduct.Name.ToLower().Contains("крышка"))?.SemiproductId;

			semiproductContent.BoxBackId = semiProducts
			  .FirstOrDefault(sProduct => sProduct.ProductId == productId && sProduct.SemiproductTypeId == (int)SemiProductType.Box && !string.IsNullOrWhiteSpace(sProduct.Name)
				&& sProduct.Name.ToLower().Contains("дно"))?.SemiproductId;

			semiproductContent.CursorId = semiProducts
			  .FirstOrDefault(sProduct => sProduct.ProductId == productId && sProduct.SemiproductTypeId == (int)SemiProductType.Cursor)?.SemiproductId;

			semiproductContent.PointerId = semiProducts
			  .FirstOrDefault(sProduct => sProduct.ProductId == productId && sProduct.SemiproductTypeId == (int)SemiProductType.Pointer)?.SemiproductId;
			return semiproductContent;
		}

		public async Task AddInventoryAsync(SemiproductWarehouseModel model)
		{
			var wHouse = new OptWarehouse
			{
				DateAdd = model.Date,
				WarehouseActionTypeId = model.WarehouseActionTypeId,
				WarehouseTypeId = model.WarehouseTypeId
			};

			_context.OptWarehouse.Add(wHouse);
			foreach (var item in model.SemiproductWarehouseFullItems)
			{
				if (wHouse.WarehouseTypeId == (int)WarehouseType.SemiproductReady)
				{
					AddIfNotEmpty(item.BlockId, item.BlockAmount, wHouse);
					AddIfNotEmpty(item.CoversId, item.CoversAmount, wHouse);
					AddIfNotEmpty(item.BoxFrontId, item.BoxFrontAmount, wHouse);
					AddIfNotEmpty(item.BoxBackId, item.BoxBackAmount, wHouse);
					AddIfNotEmpty(item.CursorId, item.CursorAmount, wHouse);
				}
				AddIfNotEmpty(item.StickersId, item.StickersAmount, wHouse);
			}
			await _context.SaveChangesAsync();
		}

		public async Task<IEnumerable<CalcSemiproductsWhjDetails>> CalculateWhJournalSemiproducts()
		{
			GetInventoriesSemiproductReady(out var lastInventory, out var partialInventories);

			var productGroups
				= await _productManager
				.GetAsync(false, dontShowDisabled: false, onlyNotebooks: true);
			var semiproducts = await _semiproductManager.GetSemiproductsWithTypes();

			var calcInfo = new List<CalcSemiproductsWhjDetails>();
			foreach (var group in productGroups)
			{
				foreach (var product in group.Value)
				{
					var semiproductWhjDetail = new CalcSemiproductsWhjDetails();
					semiproductWhjDetail.SeriesName = group.Key.Name;
					semiproductWhjDetail.ProductId = product.ProductId;
					semiproductWhjDetail.ProductName = product.DisplayName;

					var semiproductsByProductId = semiproducts
						.Where(semiproduct => semiproduct.ProductId == product.ProductId).ToList();

					foreach (var semiproduct in semiproductsByProductId)
					{
						var semiproductInfo = CalcSemiproductState(lastInventory, partialInventories, semiproduct);
						semiproductWhjDetail.SemiproductList.Add(semiproductInfo);
					}
					calcInfo.Add(semiproductWhjDetail);
				}
			}
			return calcInfo;
		}

		private void GetInventoriesSemiproductReady(out OptWarehouse lastInventory, out IQueryable<OptWarehouse> partialInventories)
		{
			lastInventory = _warehouseRepository.GetLatestInventory(WarehouseType.SemiproductReady);
			partialInventories = _warehouseRepository.GetWarehousesByWhTypeAndActionType(WarehouseType.SemiproductReady, WarehouseActionType.SingleInventory);
			if (lastInventory != null)
			{
				var dateAdd = lastInventory.DateAdd;
				partialInventories = partialInventories.Where(inv => inv.DateAdd >= dateAdd);
			}
		}

		public SemiproductPartDetails CalcSemiproductState(OptSemiproduct semiproduct)
		{
			OptWarehouse lastInventory;
			IQueryable<OptWarehouse> partialInventories;
			GetInventoriesSemiproductReady(out lastInventory, out partialInventories);
			return CalcSemiproductState(lastInventory, partialInventories, semiproduct);
		}

		private SemiproductPartDetails CalcSemiproductState(OptWarehouse lastInventory, IQueryable<OptWarehouse> partialInventories, OptSemiproduct semiproduct)
		{
			var semiproductDetails = new SemiproductPartDetails();
			semiproductDetails.SemiproductId = semiproduct.SemiproductId;
			semiproductDetails.SemiproductType = semiproduct.SemiproductTypeId;
			var semiproductInLastInventory = lastInventory.WarehouseItems.FirstOrDefault(x => x.ObjectId == semiproduct.SemiproductId);
			semiproductDetails.Inventory = semiproductInLastInventory != null ? semiproductInLastInventory.Amount : 0;

			var partialInventory = partialInventories.Where(inv => inv.WarehouseItems.Any(item => item.ObjectId == semiproduct.SemiproductId)).OrderByDescending(inv => inv.DateAdd)
				.FirstOrDefault();
			var partialInventoryItem = partialInventory?.WarehouseItems.First(item => item.ObjectId == semiproduct.SemiproductId);

			semiproductDetails.SingleInventory = partialInventoryItem?.Amount;
			var minDate = partialInventory?.DateAdd
				?? lastInventory?.DateAdd ?? DateTime.MinValue;
			semiproductDetails.Journal = _whJournalManager.Get(minDate, new List<long> { semiproduct.SemiproductId }, WhJournalObjectType.Semiproduct, WarehouseType.SemiproductReady);
			if (semiproduct.SemiproductTypeId == (int)SemiProductType.Box && !string.IsNullOrWhiteSpace(semiproduct.Name) && semiproduct.Name.ToUpper().Contains("ДНО"))
				semiproductDetails.IsBack = true;
			return semiproductDetails;
		}

		public async Task<byte[]> GetReportAsync()
		{
			var sw = new Stopwatch();
			sw.Start();
			AddStopWatchLog(sw, "start");
			byte[] bytes = new byte[0];
			int maxCols = 17;
			var semiproductTypes = Enum.GetValues(typeof(SemiProductType))
				.Cast<SemiProductType>().Where(enumValue => enumValue != SemiProductType.None);

			var productGroups = await _productManager.GetAsync(false, dontShowDisabled: false, onlyNotebooks: true);
			var rawSemiProductsList = _context
					.OptSemiproduct
					.Include(t => t.SemiproductType)
					.Include(t => t.Product)
						.ThenInclude(product => product.Country)
					.ToList();
			AddStopWatchLog(sw, "before calcDetails");
			var calcDetails = await _calcManager.CalculateFromWhjAsync(productGroups);
			AddStopWatchLog(sw, "product calcDetails done");

			using (ExcelPackage objExcelPackage = new ExcelPackage())
			{
				ExcelWorksheet objWorksheet = objExcelPackage.Workbook.Worksheets.Add("Отчёт");

				var currentRow = 1;
				var currentCol = 2;
				SetHeader(objWorksheet, currentRow, ref currentCol);

				OptWarehouse lastInventory = _warehouseRepository.GetLatestInventory(WarehouseType.SemiproductReady);

			
				IEnumerable<OptWarehouse> allPartialInventories =
					_warehouseRepository.GetWarehouseByType((int)WarehouseActionType.SingleInventory, (int)WhJournalObjectType.Semiproduct)
				   .ToList();

				List<OptPrintOrder> allNotAssembledPrintOrders = _context.OptPrintOrder
					  .Include(printOrder => printOrder.PrintOrderSemiproducts)
							.ThenInclude(printOrderSm => printOrderSm.SemiProduct)
					  .Include(printOrder => printOrder.PrintOrderIncomings)
						  .ThenInclude(printOrderInc => printOrderInc.PrintOrderIncomingItems)
				  .Where(printOrder => !(printOrder.IsAssembled ?? false)).ToList();
				AddStopWatchLog(sw, "all available lists are gotten");
				try
				{
					int? previousCountry = null;
					int? currentCountry = null;
					foreach (var group in productGroups)
					{
						AddStopWatchLog(sw, $"start group {group.Key}");
						currentCol = 1;
						var firstNotebook = group.Value.FirstOrDefault();
						currentCountry = firstNotebook?.CountryId;
						if (previousCountry != currentCountry)
						{
							DrawDivider(maxCols, objWorksheet, firstNotebook.CountryPrefix, currentCol, currentCountry, ref previousCountry, ref currentRow);
						}
						currentRow++;
						objWorksheet.Cells[currentRow, currentCol].Style.Font.Bold = true;
						objWorksheet.Cells[currentRow, currentCol].Value = group.Key.Name;

						foreach (var product in group.Value)
						{
							AddStopWatchLog(sw, $"start product {product.Name}");
							currentRow++;
							currentCol = 1;
							objWorksheet.Cells[currentRow, currentCol++].Value = product.DisplayName;
							var semiproductsJournal = (await CalcSemiproductWhjournalAsync(product.ProductId,
								lastInventory, null, allPartialInventories, allNotAssembledPrintOrders, sw)).Value;

							AddStopWatchLog(sw, $"semiproduct journal calced");

							objWorksheet.Cells[currentRow, currentCol++].Value = calcDetails.FirstOrDefault(det => det.ProductId == product.ProductId)?.Current ?? 0;
							var semiproductContent = GetSemiproductIds(rawSemiProductsList, product.ProductId);

							if (!product.IsKit)
							{
								PrintSemiproductInfo(semiproductsJournal, objWorksheet, currentRow, currentCol, semiproductContent.BlockId);
							}

							currentCol += 2;
							objWorksheet.Cells[currentRow, currentCol++].Value = semiproductContent.StickersId;
							PrintSemiproductInfo(semiproductsJournal, objWorksheet, currentRow, currentCol, semiproductContent.StickersId);
							currentCol += 2;
							PrintStickersNotchInfo(semiproductsJournal, objWorksheet, currentRow, currentCol, semiproductContent.StickersId);

							currentCol += 2;

							if (!product.IsKit)
							{
								PrintSemiproductInfo(semiproductsJournal, objWorksheet, currentRow, currentCol, semiproductContent.CoversId);
								currentCol += 4;
							}
							else
							{
								currentCol += 2;

								PrintSemiproductInfo(semiproductsJournal, objWorksheet, currentRow, currentCol, semiproductContent.BoxFrontId);

								currentCol += 2;
								PrintSemiproductInfo(semiproductsJournal, objWorksheet, currentRow, currentCol, semiproductContent.BoxBackId);
							}

							currentCol += 2;

							PrintSemiproductInfo(semiproductsJournal, objWorksheet, currentRow, currentCol, semiproductContent.CursorId, false);
							currentCol += 2;

							PrintSemiproductInfo(semiproductsJournal, objWorksheet, currentRow, currentCol, semiproductContent.PointerId, false);

							AddStopWatchLog(sw, "product cycle done");
						}
						AddStopWatchLog(sw, "group cycle done");
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "ошибка при подготовке отчёта по складу полуфабрикатов");
				}

				SetColumnWidth(objWorksheet);

				bytes = objExcelPackage.GetAsByteArray();
			}
			sw.Stop();
			return bytes;
		}

		private IEnumerable<OptWarehouseJournal> GetAllJournal(DateTime minDate)
		{
			return _context.OptWarehouseJournal
		  .Where(whj =>
				whj.ObjectTypeId == (int)WhJournalObjectType.Semiproduct && whj.ActionDate >= minDate).ToList();

		}

		private DateTime GetMinDate(OptWarehouse lastInventory, OptWarehouse lastUnNotchedInventory = null, OptWarehouse lastNochingInventory = null)
		{
			lastUnNotchedInventory = lastUnNotchedInventory ?? _warehouseRepository.GetLatestInventory(WarehouseType.StickersUnNotched);
			lastNochingInventory = lastNochingInventory ?? _warehouseRepository.GetLatestInventory(WarehouseType.StickersNotching);
			var minDates = new List<DateTime>();
			if (lastInventory != null)
			{
				minDates.Add(lastInventory.DateAdd);
			}
			if (lastUnNotchedInventory != null)
			{
				minDates.Add(lastUnNotchedInventory.DateAdd);
			}
			if (lastNochingInventory != null)
			{
				minDates.Add(lastNochingInventory.DateAdd);
			}
			var minDate = minDates.Any() ? minDates.OrderBy(date => date).First() : DateTime.MinValue;
			return minDate;
		}

		// DrawDevider(maxCols, objWorksheet, firstNotebook, currentCol, currentCountry, ref previousContry, ref currentRow);
		private static int? DrawDivider(int maxCols, ExcelWorksheet objWorksheet, string countryName, int currentCol, int? currentCountry, ref int? previousCountry, ref int currentRow)
		{
			currentRow += 2;
			objWorksheet.Cells[currentRow, currentCol].Style.Font.Bold = true;
			objWorksheet.Cells[currentRow, currentCol, currentRow, maxCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
			objWorksheet.Cells[currentRow, currentCol, currentRow, maxCols].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#add8e6"));
			objWorksheet.Cells[currentRow, currentCol].Value = countryName;
			previousCountry = currentCountry;
			return previousCountry;
		}

		private static void SetColumnWidth(ExcelWorksheet objWorksheet)
		{
			objWorksheet.Column(1).Width = 46;
			objWorksheet.Column(2).Width = 15;
			objWorksheet.Column(3).Width = 15;
			objWorksheet.Column(4).Width = 15;
			objWorksheet.Column(5).Width = 15;
			objWorksheet.Column(6).Width = 15;
			objWorksheet.Column(7).Width = 15;
		}

		private static int SetHeader(ExcelWorksheet objWorksheet, int currentRow, ref int currentCol)
		{
			objWorksheet.Cells[currentRow, currentCol++].Value = "Тетрадь, в наличии";

			objWorksheet.Cells[currentRow, currentCol++].Value = "Блок, в наличии";
			objWorksheet.Cells[currentRow, currentCol++].Value = "Блок, в печати";

			objWorksheet.Cells[currentRow, currentCol++].Value = "Наклейки, Id";
			objWorksheet.Cells[currentRow, currentCol++].Value = "Наклейки, надсечённые в наличии";
			objWorksheet.Cells[currentRow, currentCol++].Value = "Наклейки, в печати";
			objWorksheet.Cells[currentRow, currentCol++].Value = "Наклейки, не надсечённые";
			objWorksheet.Cells[currentRow, currentCol++].Value = "Наклейки, в надсечке";


			objWorksheet.Cells[currentRow, currentCol++].Value = "Обложки, в наличии";
			objWorksheet.Cells[currentRow, currentCol++].Value = "Обложки, в печати";

			objWorksheet.Cells[currentRow, currentCol++].Value = "Коробки, крышки, в наличии";
			objWorksheet.Cells[currentRow, currentCol++].Value = "Коробки, крышки, в печати";

			objWorksheet.Cells[currentRow, currentCol++].Value = "Коробки, дно, в наличии";
			objWorksheet.Cells[currentRow, currentCol++].Value = "Коробки, дно, в печати";

			objWorksheet.Cells[currentRow, currentCol++].Value = "Курсоры, в наличии";
			objWorksheet.Cells[currentRow, currentCol++].Value = "Курсоры, в печати";

			objWorksheet.Cells[currentRow, currentCol++].Value = "Пойнтеры, в наличии";
			objWorksheet.Cells[currentRow, currentCol++].Value = "Пойнтеры, в печати";
			objWorksheet.Cells[currentRow, 2, currentRow, currentCol].Style.WrapText = true;
			return currentCol;
		}

		private void PrintStickersNotchInfo(IEnumerable<SemiproductCalcDetails> semiproductsJournal, ExcelWorksheet objWorksheet, int currentRow, int currentCol, int? stickersId)
		{
			if (stickersId.HasValue)
			{
				var thisSemiproductDetails = semiproductsJournal?.FirstOrDefault(t => t.SemiproductId == stickersId);

				objWorksheet.Cells[currentRow, currentCol++].Value = thisSemiproductDetails?.StickersUnNotchedSum;
				objWorksheet.Cells[currentRow, currentCol].Value = thisSemiproductDetails?.StickersNotchingSum;
			}
			else
			{
				PrintNoData(objWorksheet, currentRow, currentCol);
			}
		}

		public IQueryable<OptSemiproduct> GetSemiproductsByProductId(int productId)
		{
			return _context.OptSemiproduct.Where(semiproduct => semiproduct.ProductId == productId).Include(x => x.SemiproductType);
		}

		public IQueryable<OptSemiproduct> GetSemiproductsForKit(int kitId)
		{
			var semiprods = _context.OptSemiproduct.Where(x => x.ProductId == kitId);
			var product = _productManager.GetDetails(kitId);

			if (product.ProductSeriesId == Const.CalendarSeriedId)
			{
				semiprods = semiprods.Where(sp => sp.SemiproductTypeId == (int)SemiProductType.Stickers || sp.SemiproductTypeId == (int)SemiProductType.Cursor);
			}
			return semiprods;
		}


		private static void PrintNoData(ExcelWorksheet objWorksheet, int currentRow, int currentCol)
		{
			var startCol = currentCol;
			var endCol = ++startCol;
			objWorksheet.Cells[currentRow, startCol, currentRow, endCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
			objWorksheet.Cells[currentRow, startCol, currentRow, endCol].Style.Fill.BackgroundColor.SetColor(_noSemiproductColor);
			objWorksheet.Cells[currentRow, startCol, currentRow, endCol].Value = "не указан полуфабрикат";
		}


		private void PrintSemiproductInfo(IEnumerable<SemiproductCalcDetails> details, ExcelWorksheet objWorksheet, int currentRow, int currentCol, long? semiproductId, bool printWarniing = true)
		{
			if (semiproductId.HasValue)
			{
				var thisSemiproductDetails = details?.FirstOrDefault(t => t.SemiproductId == semiproductId);

				objWorksheet.Cells[currentRow, currentCol++].Value = thisSemiproductDetails?.Current;
				objWorksheet.Cells[currentRow, currentCol].Value = thisSemiproductDetails?.PrintingInProcess;
			}
			if (!semiproductId.HasValue & printWarniing)
			{
				PrintNoData(objWorksheet, currentRow, currentCol);
			}
		}


		private void AddIfNotEmpty(int? semiproductId, int? amount, OptWarehouse wHouse)
		{
			if (semiproductId.HasValue && amount.HasValue && amount > 0)
			{
				var wHouseItem = new OptWarehouseItem
				{
					Amount = amount.Value,
					ObjectId = semiproductId.Value,
					Warehouse = wHouse
				};
				_context.Add(wHouseItem);
			}
		}

		private SemiproductWarehouseFullModel SetSemiproductAmount(SemiproductWarehouseFullModel item, List<OptWarehouseItem> warehouseItems)
		{
			item.BlockAmount = warehouseItems.FirstOrDefault(smprod => smprod.ObjectId == item.BlockId)?.Amount;
			item.StickersAmount = warehouseItems.FirstOrDefault(smprod => smprod.ObjectId == item.StickersId)?.Amount;
			item.CoversAmount = warehouseItems.FirstOrDefault(smprod => smprod.ObjectId == item.CoversId)?.Amount;
			item.BoxFrontAmount = warehouseItems.FirstOrDefault(smprod => smprod.ObjectId == item.BoxFrontId)?.Amount;
			item.BoxBackAmount = warehouseItems.FirstOrDefault(smprod => smprod.ObjectId == item.BoxBackId)?.Amount;
			item.CursorAmount = warehouseItems.FirstOrDefault(smprod => smprod.ObjectId == item.CursorId)?.Amount;
			return item;
		}

		private void UpdateIfNotEmpty(int? semiproductId, int? amount, IQueryable<OptWarehouseItem> rawItems, OptWarehouse raw)
		{
			if (semiproductId.HasValue && amount.HasValue)
			{
				var smWhouseItem = rawItems.FirstOrDefault(smwhItem => smwhItem.ObjectId == semiproductId.Value);
				if (smWhouseItem == null)
				{
					smWhouseItem = new OptWarehouseItem
					{
						Amount = amount.Value,
						ObjectId = semiproductId.Value,
						Warehouse = raw
					};
					_context.Add(smWhouseItem);
				}
				else
				{
					smWhouseItem.Amount = amount.Value;
					_context.Update(smWhouseItem);
				}
			}
		}

		private async Task AddPartialToSemiproductWarehouse(int objectId, int amount, string comment)
		{
			var wHouse = new OptSemiproductWarehouse
			{
				Date = DateTime.Now,
				WarehouseActionTypeId = (int)WarehouseActionType.SingleInventory,
				Comment = comment
			};

			var item = new OptSemiproductWarehouseItem
			{
				SemiproductId = objectId,
				Amount = amount,
				SemiproductWarehouse = wHouse,
			};

			await _context.OptSemiProductWarehouse.AddAsync(wHouse);
			await _context.OptSemiproductWarehouseItem.AddAsync(item);
		}

		private async Task AddPartialToWarehouse(PartialInventoryModel model, string comment)
		{
			var wHouse = new OptWarehouse
			{
				DateAdd = DateTime.Now,
				WarehouseTypeId = model.WarehouseTypeId,
				WarehouseActionTypeId = (int)WarehouseActionType.SingleInventory,
				Comment = comment
			};

			var item = new OptWarehouseItem
			{
				ObjectId = model.SemiproductId,
				Amount = model.Amount,
				Warehouse = wHouse,
			};

			await _context.OptWarehouse.AddAsync(wHouse);
			await _context.OptWarehouseItem.AddAsync(item);
		}

		public async Task DeleteAsync(int id)
		{

			var semiproductWarehouse = await _context.OptSemiProductWarehouse.FirstOrDefaultAsync(smWarehouse => smWarehouse.SemiproductWarehouseId == id);
			_context.Remove(semiproductWarehouse);
			await _context.SaveChangesAsync();
		}
	}
}