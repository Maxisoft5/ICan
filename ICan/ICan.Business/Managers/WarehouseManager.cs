using AutoMapper;
using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using ICan.Data.Context;
using ICan.Common.Utils;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class WarehouseManager : BaseManager
	{
		protected readonly ProductManager _productManager;
		protected readonly IProductRepository _productRepository;
		protected readonly SemiproductWarehouseManager _semiProdsManager;
		protected readonly CalcManager _calcManager;
		protected readonly WarehouseJournalManager _whJournalManager;
		protected readonly IWarehouseRepository _warehouseRepository;
		protected readonly CommonManager<OptPaper> _paperManager;

		protected virtual List<int> _excludedActions { get; set; }
		protected virtual List<int> _fullInfoActions { get; set; }

		public WarehouseManager(IMapper mapper, IWarehouseRepository warehouseRepository,
			IProductRepository productRepository,
			ApplicationDbContext context, ILogger<BaseManager> logger,
			  ProductManager productManager,
			   WarehouseJournalManager whJournalManager,
			   SemiproductWarehouseManager semiproductWarehouseManager,
			 CalcManager calcManager,
			 CommonManager<OptPaper> paperManager
		) : base(mapper, context, logger)
		{
			_productManager = productManager;
			_productRepository = productRepository;
			_calcManager = calcManager;
			_whJournalManager = whJournalManager;
			_semiProdsManager = semiproductWarehouseManager;
			_paperManager = paperManager;
			_warehouseRepository = warehouseRepository;
			_excludedActions = new List<int> { (int)WarehouseActionType.Correction, (int)WarehouseActionType.Snapshot };
			_fullInfoActions = new List<int> { (int)WarehouseActionType.Inventory, (int)WarehouseActionType.Snapshot };
		}

		public IEnumerable<WarehouseModel> Get(WhJournalObjectType notebook, TableOptions options, out int total)
		{
			var waehouses = GetWByObjectType(notebook);
			var pageSize = int.TryParse(options?.Limit, out var pageS) ? pageS : Const.PageSize;
			var offset = int.TryParse(options?.Offset, out var offS) ? offS : 0;
			if (!string.IsNullOrWhiteSpace(options?.Filter))
			{
				waehouses = Filter(waehouses, options.Filter);
			}
			if (!string.IsNullOrWhiteSpace(options?.Sort))
			{
				waehouses = Sort(waehouses, options);
			}
			total = waehouses.Count();

			var list = waehouses.Skip(offset).Take(pageSize).ToList();
			var model = _mapper.Map<IEnumerable<WarehouseModel>>(list);
			return model;
		}

		private IQueryable<OptWarehouse> Sort(IQueryable<OptWarehouse> warehouses, TableOptions options)
		{
			//Func<OptWarehouse, IQueryable<OptWarehouse>> orderFunc = 
			//	(wh)=> OrderBy()
			if (options.Sort == "warehouseActionTypeName")
			{
				if (options.SortOrder.Equals("desc"))
					return warehouses.OrderByDescending(wh => wh.WarehouseActionType.Name);
				return warehouses.OrderBy(wh => wh.WarehouseActionType.Name);
			}

			return warehouses;
		}

		private IQueryable<OptWarehouse> Filter(IQueryable<OptWarehouse> warehouses, string filterString)
		{
			var filter = JsonConvert.DeserializeObject<WarehouseFilter>(filterString);

			if (!string.IsNullOrWhiteSpace(filter.WarehouseActionTypeId))
			{
				var action = int.Parse(filter.WarehouseActionTypeId);
				warehouses = warehouses.Where(wh => wh.WarehouseActionTypeId == action);
			}
			if (!string.IsNullOrWhiteSpace(filter.Comment))
			{
				var template = filter.Comment.ToLower();
				warehouses = warehouses.Where(wh => wh.Comment.Contains(template));
			}
			if (!string.IsNullOrWhiteSpace(filter.SoleItemAmount))
			{
				if (int.TryParse(filter.SoleItemAmount.ToLower(), out var amount))
					warehouses = warehouses.Where(wh => wh.WarehouseItems.Count() == 1 && wh.WarehouseItems.First().Amount == amount);
			}
			if (!string.IsNullOrWhiteSpace(filter.SoleItemName))
			{
				if (int.TryParse(filter.SoleItemName.ToLower(), out var productId))
					warehouses = warehouses.Where(wh => wh.WarehouseItems.Count() == 1 && wh.WarehouseItems.First().ProductId == productId);
			}

			return warehouses;
		}

		public virtual async Task SetListsAsync(WarehouseModel model)
		{
			model.WarehouseActionTypes = _calcManager.GetActions(excludedActions: _excludedActions);
			model.Assemblies = GetAssemblies();
			model.Items = await _productManager.GetAsync(false, dontShowDisabled: false, onlyNotebooks: false);
			var alreadySetProducts = model.WarehouseItems?.Where(prod => prod.Amount > 0).ToList();
			if (alreadySetProducts != null)
			{
				var items = model.Items.SelectMany(item => item.Value);
				foreach (var alreadySetProduct in alreadySetProducts)
				{
					var found = items.First(prod => prod.ProductId == alreadySetProduct.ProductId);
					found.Amount = alreadySetProduct.Amount;
				}
			}
		}

		public async Task<byte[]> GetWhStateFile()
		{
			var model = await GetWarehouseState();
			byte[] bytes = new byte[0];
			try
			{
				using (ExcelPackage objExcelPackage = new ExcelPackage())
				{
					ExcelWorksheet objWorksheet = objExcelPackage.Workbook.Worksheets.Add("Состояние склада");

					var current = 1;

					objWorksheet.Cells[current, 1].Value = "Тетрадь";
					objWorksheet.Cells[current, 1].Style.Font.Bold = true;
					objWorksheet.Cells[current, 2].Value = "Количество";
					objWorksheet.Cells[current, 2].Style.Font.Bold = true;

					foreach (var group in model.Items)
					{

						objWorksheet.Cells[++current, 1, current, 2].Style.Font.Bold = true;
						objWorksheet.Cells[current, 1, current, 2].Style.Fill.PatternType = ExcelFillStyle.LightGray;
						objWorksheet.Cells[current, 1, current, 2].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
						objWorksheet.Cells[current, 1].Value = group.Key.Name;
						foreach (var notebook in group.Value)
						{
							objWorksheet.Cells[++current, 1].Value = notebook.DisplayName;
							objWorksheet.Cells[current, 2].Value = notebook.Amount;
							objWorksheet.Cells[current, 2].Style.Numberformat.Format
					   = "#,##0";
						}
					}

					objWorksheet.Column(1).Width = 60;
					objWorksheet.Column(2).Width = 15;

					var borderStyle = ExcelBorderStyle.Medium;
					var color = Color.Black;
					var rangeStyle = objWorksheet.Cells[1, 1, current, 2].Style;
					SetBorder(borderStyle, color, rangeStyle.Border.Left);
					SetBorder(borderStyle, color, rangeStyle.Border.Right);
					SetBorder(borderStyle, color, rangeStyle.Border.Top);
					SetBorder(borderStyle, color, rangeStyle.Border.Bottom);

					bytes = objExcelPackage.GetAsByteArray();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[GetWhStateFile]");
			}
			return bytes;
		}

		private static void SetBorder(ExcelBorderStyle borderStyle, Color color, ExcelBorderItem styleItem)
		{
			styleItem.Style = borderStyle;
			styleItem.Color.SetColor(color);
		}

		public async Task<WarehouseStateModel> GetWarehouseState()
		{
			var productGroups = await _productManager
				.GetAsync(false, dontShowDisabled: false, onlyNotebooks: false);
			var calcDetails = await _calcManager.CalculateFromWhjAsync(productGroups);

			foreach (var productGroup in productGroups)
			{
				productGroup.Value.ForEach(product =>
					product.Amount = calcDetails.FirstOrDefault(det => det.ProductId == product.ProductId)?.Current ?? 0);
			};

			var model = new WarehouseStateModel
			{
				Items = productGroups
			};
			return model;
		}

		public virtual async Task<WarehouseModel> GetDetailsModelAsync(OptWarehouse raw, ActionType actionType)
		{
			var model = _mapper.Map<WarehouseModel>(raw);
			model.AssemblyId = raw.Assembly?.AssemblyId;
			model.Assemblies = GetAssemblies(true);
			if (model != null)
			{
				model.Items = await GetModelProducts(raw, actionType);
				model.WarehouseActionTypes = _calcManager.GetActions(excludedActions: _excludedActions);
			}
			return model;
		}

		public virtual async Task Update(WarehouseModel model)
		{
			var warehouse = _context.OptWarehouse.Include(wh => wh.WarehouseItems)
				.First(wh => wh.WarehouseId == model.WarehouseId);
			warehouse.DateAdd = model.DateAdd;
			warehouse.Comment = model.Comment;

			foreach (var product in model.WarehouseItems)
			{
				var whItem = warehouse.WarehouseItems.FirstOrDefault(item => item.ProductId == product.ProductId);
				if (whItem != null)
				{
					whItem.Amount = product.Amount;
				}
				else
				{
					var newItem = new OptWarehouseItem
					{
						ProductId = product.ProductId,
						Amount = product.Amount,
						Warehouse = warehouse
					};
					_context.Add(newItem);
				}

			}
			_context.Update(warehouse);
			if (warehouse.WarehouseActionTypeId == (int)WarehouseActionType.Returning)
			{
				await UpdateReturning(warehouse, model);
			}
			await _context.SaveChangesAsync();
		}

		private async Task UpdateReturning(OptWarehouse warehouse, WarehouseModel model)
		{
			await _whJournalManager.RemoveByAction(warehouse);
			var now = DateTime.Now;
			var list = model.WarehouseItems
					.Where(whItem => whItem.Amount > 0)
					.Select(whItem => new WarehouseJournalModel
					{
						ObjectId = (long)whItem.ProductId,
						ActionTypeId = WhJournalActionType.Income,
						ActionExtendedTypeId = WhJournalActionExtendedType.Returning,
						ActionId = model.WarehouseId.ToString(),
						Amount = whItem.Amount,
						ActionDate = now,
						ObjectTypeId = WhJournalObjectType.Notebook,
						WarehouseTypeId = WarehouseType.NotebookReady
					});

			await _whJournalManager.AddRangeAsync(list, false);
		}

		public async Task<IEnumerable<WarehouseModel>> Get(WhJournalObjectType objectType)
		{
			var rawList = GetWByObjectType(objectType);

			var warehouseEntries = _mapper.Map<IEnumerable<WarehouseModel>>(rawList);

			return warehouseEntries;
		}

		// public async Task<> {} 

		private IQueryable<OptWarehouse> GetWByObjectType(WhJournalObjectType objectType)
		{
			var startDate = DateTime.Now.AddYears(-1);
			return _context.OptWarehouse
							.Include(warehouse => warehouse.WarehouseType)
							.Include(warehouse => warehouse.WarehouseItems)
								.ThenInclude(warehouseItem => warehouseItem.Product)
									.ThenInclude(prod => prod.Country)
							.Include(warehouse => warehouse.WarehouseItems)
								.ThenInclude(warehouseItem => warehouseItem.Product)
									.ThenInclude(prod => prod.ProductSeries)
							.Include(warehouse => warehouse.WarehouseActionType)
							.Include(warehouse => warehouse.Assembly)
							.Where(warehouse =>
							warehouse.DateAdd >= startDate &&
							warehouse.WarehouseType.WarehouseObjectType == (int)objectType && !_excludedActions.Contains(warehouse.WarehouseActionTypeId)).OrderByDescending(wh => wh.DateAdd);
		}

		public IEnumerable<SelectListItem> GetWarehousesTypesByWhObjectType(WhJournalObjectType whObjectType, WarehouseType selected)
		{
			var rawList = _warehouseRepository.GetWarehousesTypesByWhObjectType(whObjectType).ToList();
			var list = rawList.Select(wh => new SelectListItem { Text = wh.Name, Value = wh.WarehouseTypeId.ToString(), Selected = wh.WarehouseTypeId == (int)selected });
			return list;
		}

		public void CheckAssemblyIsAvailable(WarehouseModel model)
		{
			var others = _context.OptAssembly
				.Include(asmbly => asmbly.Warehouse)
			   .Where(asmbly => asmbly.WarehouseId.HasValue && asmbly.AssemblyId == model.AssemblyId
			   && (model.WarehouseId == 0 || model.WarehouseId != asmbly.WarehouseId)).ToList();
			if (others.Any())
			{
				var whError = string.Join(", ", others.Select(asmbly => asmbly.Warehouse.DateAdd.ToShortDateString()));
				throw new UserException($"Невозможно использовать эту сборку, она уже используется в приходах от {whError}.");
			}
		}

		public IEnumerable<SelectListItem> GetActions()
		{
			return _calcManager.GetActions(excludedActions: _excludedActions);
		}

		public async Task<bool> Create(WarehouseModel model)
		{
			if (model.WarehouseActionTypeId == (int)WarehouseActionType.SingleInventory)
			{
				var defaultComment = await GetSingleInventoryCommentAsync(model);
				model.Comment = $"{defaultComment}. Комментарий: {model.Comment}";
			}
			var whjObjectType = _warehouseRepository.GetWhjObjectTypeByWh(model.WarehouseTypeId);
			var inventory = await _calcManager.GetLatestInventoryAsync(whjObjectType);

			OptWarehouse warehouse = null;
			List<WarehouseJournalModel> journal = new List<WarehouseJournalModel>();
			var successs = false;
			try
			{
				warehouse = new OptWarehouse
				{
					DateAdd = model.DateAdd,
					WarehouseActionTypeId = model.WarehouseActionTypeId,
					Comment = model.Comment,
					WarehouseTypeId = (int)model.WarehouseTypeId,
					WarehouseId = model.WarehouseId //for testing purposes 
				};
				var warehouseId = await _warehouseRepository.CreateAsync(warehouse);

				if (model.WarehouseActionTypeId == (int)WarehouseActionType.Arrival)
				{
					var assembly = _context.OptAssembly
						.Include(ass => ass.Product)
						.First(ass => ass.AssemblyId == model.AssemblyId.Value);
					assembly.Warehouse = warehouse;
					var rawItem = new OptWarehouseItem
					{
						ProductId = assembly.ProductId,
						Amount = assembly.Amount,
						WarehouseId = warehouseId
					};
					await _warehouseRepository.CreateItemAsync(rawItem);
					if (assembly.Product.ProductSeriesId != Const.CalendarSeriedId)
					{
						journal.Add(new WarehouseJournalModel
						{
							ActionDate = model.DateAdd,
							ActionTypeId = WhJournalActionType.Income,
							ActionExtendedTypeId = WhJournalActionExtendedType.WarehouseArrival,
							ObjectTypeId = WhJournalObjectType.Notebook,
							ObjectId = assembly.ProductId,
							Amount = assembly.Amount,
							WarehouseTypeId = WarehouseType.NotebookReady,
							ActionId = warehouseId.ToString()
						});
					}
				}
				else if (model.WarehouseActionTypeId == (int)WarehouseActionType.SingleInventory)
				{
					var changedProduct = model.WarehouseItems.First(item => item.Amount > 0);
					var rawItem = new OptWarehouseItem
					{
						ProductId = changedProduct.ProductId,
						ObjectId = changedProduct.ObjectId,
						WarehouseId = warehouseId,
						Amount = changedProduct.Amount
					};
					await _warehouseRepository.CreateItemAsync(rawItem);
				}
				else
				{
					await AddWarehouseItemsAsync(model, warehouseId, journal, inventory);
				}

				journal.ForEach(jEntry => jEntry.ActionId = warehouse.WarehouseId.ToString());
				await _whJournalManager.AddRangeAsync(journal);
				await _context.SaveChangesAsync();
				successs = true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"[Warehouse] error in warehouse");

				await CleanDataAsync(warehouse, journal);
				throw;
			}
			return successs;
		}

		public async Task Delete(int id, bool isAdmin, bool isAssembler  = false)
		{
			var whouse = await _context.OptWarehouse.FirstOrDefaultAsync(m => m.WarehouseId == id);
			if (whouse.WarehouseActionTypeId == (int)WarehouseActionType.Inventory 
				|| whouse.WarehouseActionTypeId == (int)WarehouseActionType.SingleInventory)
			{
				var timeGap = (DateTime.Now - whouse.DateAdd);
				var canGo = 
					isAdmin || (isAssembler && timeGap < Const.DeletePeriod);
				if (!canGo)
					throw new UserException(Const.ErrorMessages.UnsufficientRights);
			}
			await _whJournalManager.RemoveByAction(whouse);
			_context.OptWarehouse.Remove(whouse);
			await _context.SaveChangesAsync();
		}

		public async Task<OptWarehouse> GetWarehouseDetailsAsync(int id)
		{
			return await _context.OptWarehouse
				.Include(warehouse => warehouse.WarehouseItems)
				.Include(warehouse => warehouse.Assembly)
				.Include(warehouse => warehouse.WarehouseType)
				.Include(warehouse => warehouse.WarehouseActionType)
				.FirstAsync(wHouse => wHouse.WarehouseId == id);
		}

		public async Task<OptWarehouseType> GetWareHouseByType(WarehouseType warehouseType)
        {
			return await _warehouseRepository.GetWareHouseTypeByType(warehouseType);
		}


		protected virtual async Task<string> GetSingleInventoryCommentAsync(WarehouseModel model)
		{
			var state = await _calcManager.CalculateFromWhjAsync(null);
			var changedProduct = model.WarehouseItems.First(item => item.Amount > 0);
			var raw = _productRepository.GetDetails(changedProduct.ProductId.Value);
			var product = _mapper.Map<ProductModel>(raw);
			var oldValue = state.First(stateItem => stateItem.ProductId == changedProduct.ProductId).Current;
			return $"Изменено значение для {product.DisplayName}, было {oldValue}, стало {changedProduct.Amount}";
		}

		private async Task<IEnumerable<KeyValuePair<OptProductseries, List<ProductModel>>>> GetModelProducts(OptWarehouse raw, ActionType actionType)
		{
			IEnumerable<KeyValuePair<OptProductseries, List<ProductModel>>> products = await _productManager.GetAsync(false, dontShowDisabled: false, onlyNotebooks: false);
			raw.WarehouseItems.ToList().ForEach(item =>
			{
				foreach (var group in products)
				{
					var product = group.Value.FirstOrDefault(t => t.ProductId == item.ProductId);
					if (product != null)
						product.Amount = item.Amount;
				}
			});

			if (actionType == ActionType.Details && !_fullInfoActions.Contains(raw.WarehouseActionTypeId))
			{
				var nonEmptySeries = new List<KeyValuePair<OptProductseries, List<ProductModel>>>();
				foreach (var series in products)
				{
					var nonEmpty = series.Value.Where(product => product.Amount > 0);

					if (nonEmpty.Any())
					{
						var keyValuePair = new KeyValuePair<OptProductseries, List<ProductModel>>(series.Key, nonEmpty.ToList());
						nonEmptySeries.Add(keyValuePair);
					}
				}

				products = nonEmptySeries;
			}
			return products;
		}


		protected virtual async Task AddWarehouseItemsAsync(WarehouseModel model, int warehouseId, List<WarehouseJournalModel> journal, WarehouseModel inventory)
		{
			foreach (var product in model.WarehouseItems)
			{
				if (product.Amount > 0)
				{
					WhJournalActionType actionTypeId = WhJournalActionType.None;
					WhJournalActionExtendedType actionExtendedTypeId = WhJournalActionExtendedType.None;

					var productFromDb = _productRepository.GetDetails(product.ProductId.Value);

					if (model.WarehouseActionTypeId == (int)WarehouseActionType.KitAssembly)
					{
						var productsInKit = _productManager.GetProductsInKit(productFromDb);
						if (productFromDb.ProductSeriesId == Const.CalendarSeriedId)
						{
							journal.Add(new WarehouseJournalModel
							{
								ActionDate = model.DateAdd,
								ActionTypeId = WhJournalActionType.Outcome,
								ActionExtendedTypeId = WhJournalActionExtendedType.KitAssemblyPart,
								ObjectTypeId = WhJournalObjectType.GluePad,
								ObjectId = Const.GluePadProductId,
								Amount = product.Amount,
								WarehouseTypeId = WarehouseType.GluePads
							});
						}

						if (productsInKit != null && productsInKit.Any())
						{
							List<string> productsWithoutNeededAmount = await CheckNeededAmount(productsInKit, product.Amount, inventory);

							if (productsWithoutNeededAmount.Count > 0)
								throw new UserException($"Невозможно собрать комплект, т.к. нет необходимых составляюших ({string.Join(", ", productsWithoutNeededAmount)})");


							foreach (var kitProduct in productsInKit)
							{
								journal.Add(new WarehouseJournalModel
								{
									ActionDate = model.DateAdd,
									ActionTypeId = WhJournalActionType.Outcome,
									ActionExtendedTypeId = WhJournalActionExtendedType.KitAssemblyPart,
									ObjectTypeId = WhJournalObjectType.Notebook,
									ObjectId = kitProduct.ProductId,
									Amount = product.Amount,
									WarehouseTypeId = WarehouseType.NotebookReady
								});
							}
						}
						var semiproductsForKit = await _semiProdsManager.GetSemiproductsForKit(productFromDb.ProductId).ToListAsync();
						var semiproductsForKitIds = semiproductsForKit?.Select(semiproduct => semiproduct.SemiproductId);
						var semiproductState = await _semiProdsManager.CalcSemiproductWhjournalAsync((int)product.ProductId);

						var semiProductsInsuficient = semiproductState.Value.Where(sPr => sPr.Current < product.Amount
							&& semiproductsForKitIds.Contains(sPr.SemiproductId)).ToList();

						TryUseComplementaryStickers(semiProductsInsuficient, semiproductsForKit, product.Amount);
						if (semiProductsInsuficient.Any())
						{
							var names = string.Join(", ", semiProductsInsuficient.Select(sPr => sPr.SemiproductDisplayName));
							throw new UserException($"Невозможно собрать комплект, недостаточно полуфабрикатов {names} ");
						}
						foreach (var semiproduct in semiproductsForKit)
						{
							journal.Add(new WarehouseJournalModel
							{
								ActionDate = model.DateAdd,
								ActionTypeId = WhJournalActionType.Outcome,
								ActionExtendedTypeId = WhJournalActionExtendedType.KitAssemblyPart,
								ObjectTypeId = WhJournalObjectType.Semiproduct,
								ObjectId = semiproduct.SemiproductId,
								Amount = product.Amount,
								WarehouseTypeId = WarehouseType.SemiproductReady
							});
						}

						actionTypeId = WhJournalActionType.Income;
						actionExtendedTypeId = WhJournalActionExtendedType.KitAssembly;
					}

					if (model.WarehouseActionTypeId == (int)WarehouseActionType.Marketing)
					{
						var (currentStateJournal, singleInventory) = _calcManager.GetProductWarehouseState((int)product.ProductId, inventory?.DateAdd, WarehouseType.NotebookReady);

						var caldWjhDetails = new CalcWhjDetails
						{
							ProductId = (int)product.ProductId,
							Journal = currentStateJournal,
							SingleInventory = singleInventory?.WarehouseItems.First().Amount,
							SingleInventoryDate = singleInventory?.DateAdd,
							Inventory = inventory?.WarehouseItems.FirstOrDefault(inv => inv.ProductId == product.ProductId)?.Amount ?? 0,
							InventoryDate = inventory?.DateAdd
						};

						if (caldWjhDetails.Current < product.Amount)
							throw new UserException("Невозможно отгрузить больше тетрадей, чем есть на складе");
						actionTypeId = WhJournalActionType.Outcome;
						actionExtendedTypeId = WhJournalActionExtendedType.Marketing;
					}
					if (model.WarehouseActionTypeId == (int)WarehouseActionType.Returning)
					{
						actionTypeId = WhJournalActionType.Income;
						actionExtendedTypeId = WhJournalActionExtendedType.Returning;
					}

					var warehouseItem = new OptWarehouseItem
					{
						ProductId = product.ProductId,
						Amount = product.Amount,
						WarehouseId = warehouseId
					};
					await _warehouseRepository.CreateItemAsync(warehouseItem);

					if (model.WarehouseActionTypeId != (int)WarehouseActionType.Inventory)
					{
						journal.Add(new WarehouseJournalModel
						{
							ActionDate = model.DateAdd,
							ActionTypeId = actionTypeId,
							ActionExtendedTypeId = actionExtendedTypeId,
							ObjectTypeId = WhJournalObjectType.Notebook,
							ObjectId = (int)product.ProductId,
							Amount = product.Amount,
							WarehouseTypeId = WarehouseType.NotebookReady,
							ActionId = warehouseId.ToString()
						});
					}
				}
			}
		}

		private void TryUseComplementaryStickers(List<SemiproductCalcDetails> semiProductsInsuficient, List<OptSemiproduct> semiproductsForKit, int amount)
		{
			if (semiProductsInsuficient == null || !semiProductsInsuficient.Any())
				return;
			var stickerInKit = semiproductsForKit.FirstOrDefault(st => st.SemiproductTypeId == (int)SemiProductType.Stickers);

			var insuficientSticker = semiProductsInsuficient.FirstOrDefault(st => st.SemiproductTypeId == (int)SemiProductType.Stickers);
			if (insuficientSticker == null || stickerInKit == null)
				return;
			var productId = semiProductsInsuficient.First().ProductId;

			var universalStickers = _context.OptSemiproductProductRelation
					.Include(semiprodrel => semiprodrel.Semiproduct)
					.ThenInclude(semiprod => semiprod.SemiproductType)
				   .FirstOrDefault(semiprodrel => semiprodrel.ProductId == productId);
			if (universalStickers == null)
				return;
			semiProductsInsuficient.Remove(insuficientSticker);
			semiproductsForKit.Remove(stickerInKit);

			var state = _semiProdsManager.CalcSemiproductState(universalStickers.Semiproduct);
			if (state.CurrentAmount < amount)
			{
				semiProductsInsuficient.Add(new SemiproductCalcDetails { SemiproductDisplayName = _mapper.Map<SemiproductModel>(universalStickers.Semiproduct).DisplayName });
				return;
			}
			semiproductsForKit.Add(universalStickers.Semiproduct);
		}

		private async Task<List<string>> CheckNeededAmount(IEnumerable<ProductModel> productsInKit, int amount, WarehouseModel inventory)
		{
			var productsWithoutNeededAmount = new List<string>();

			var productIds = productsInKit
				 .Select(product => (long)product.ProductId);
			var journal = _whJournalManager.Get(inventory.DateAdd, productIds, WhJournalObjectType.Notebook, WarehouseType.NotebookReady).ToList();
			if (productIds.Contains(Const.GluePadProductId))
			{
				inventory = await _calcManager.GetLatestInventoryAsync(WhJournalObjectType.GluePad);
				journal.AddRange(_whJournalManager.Get(inventory?.DateAdd,
					new List<long> { Const.GluePadProductId }, WhJournalObjectType.GluePad, WarehouseType.GluePads));
			}

			foreach (var productInKit in productsInKit)
			{
				var inventoryAmount = inventory?.WarehouseItems
						   .FirstOrDefault(inv => inv.ProductId == productInKit.ProductId)?.Amount ?? 0;
				var singleInventory = _calcManager.GetLatestSingleInventory(productInKit.ProductId, inventory?.DateAdd, WarehouseType.NotebookReady, WhJournalObjectType.Notebook);
				var productJournal = journal.Where(item => item.ObjectId == productInKit.ProductId);

				if (singleInventory != null)
				{
					productJournal = productJournal.Where(item => item.ActionDate >= singleInventory.DateAdd);
				}
				var caldWjhDetails = new CalcWhjDetails
				{
					ProductId = productInKit.ProductId,
					Name = productInKit.DisplayName,
					IsKit = productInKit.IsKit,
					AssemblesAsKit = productInKit.AssemblesAsKit,
					ProductSeriesId = productInKit.ProductSeriesId,
					Journal = productJournal,
					Inventory = inventoryAmount,
					InventoryDate = inventory?.DateAdd,
					SingleInventoryDate = singleInventory?.DateAdd,
					SingleInventory = singleInventory?.WarehouseItems.First(whItem => whItem.ProductId == productInKit.ProductId).Amount,
				};

				if (caldWjhDetails.Current < amount)
					productsWithoutNeededAmount.Add(productInKit.Name);
			}

			return productsWithoutNeededAmount;
		}

		private IEnumerable<SelectListItem> GetAssemblies(bool all = false)
		{
			IQueryable<OptAssembly> list = _context.OptAssembly
						.Include(ass => ass.Warehouse)
						.Include(product => product.Product.Country);

			if (!all)
			{
				list = list.Where(ass => ass.Warehouse == null);
			}

			var selectList = new List<SelectListItem>();
			list.ToList().ForEach(ass =>
				{
					var product = _mapper.Map<ProductModel>(ass.Product);
					selectList.Add(new SelectListItem
					{
						Text = $"{product.DisplayName} в размере {ass.Amount} от {ass.Date.ToShortDateString()}",
						Value = ass.AssemblyId.ToString()
					});
				});
			return selectList;
		}

		private async Task CleanDataAsync(OptWarehouse warehouse, List<WarehouseJournalModel> journal)
		{
			try
			{
				if (warehouse == null || warehouse.WarehouseId == 0)
					return;

				foreach (var entry in journal)
				{
					await _whJournalManager.RemoveByAction(warehouse);
				}
				await _warehouseRepository.DeleteAsync(warehouse.WarehouseId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"[Warehouse] Error while cleaning. Possible inconsistent data");
			}
		}
	}
}
