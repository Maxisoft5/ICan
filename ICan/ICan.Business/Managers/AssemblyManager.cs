using AutoMapper;
using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using ICan.Common.Utils;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class AssemblyManager : BaseManager
	{
		private readonly IEnumerable<int> _excludedProducts = new List<int> { 47 };//букварь

		private readonly SemiproductWarehouseManager _semiproductWarehouseManager;
		private readonly PrintOrderManager _printOrderManager;
		private readonly NotchOrderManager _notchOrderManager;
		private readonly WarehouseJournalManager _whJournalManager;
		private readonly IProductRepository _productRepository;
		private readonly IAssemblyRepository _assemblyRepository;

		public AssemblyManager(IMapper mapper, ApplicationDbContext context,
			SemiproductWarehouseManager semiproductWarehouseManager,
			WarehouseJournalManager warehouseJournalManager,
			ILogger<BaseManager> logger,
			PrintOrderManager printOrderManager,
			NotchOrderManager notchOrderManager,
			IProductRepository productRepository,
			IAssemblyRepository assemblyRepository
			) : base(mapper, context, logger)
		{
			_semiproductWarehouseManager = semiproductWarehouseManager;
			_printOrderManager = printOrderManager;
			_notchOrderManager = notchOrderManager;
			_whJournalManager = warehouseJournalManager;
			_productRepository = productRepository;
			_assemblyRepository = assemblyRepository;
		}

		public async Task<long> CreateAsync(AssemblyModel model)
		{
			CheckPrintOrdersIsAssembled(model);

			var raw = new OptAssembly
			{
				Date = model.Date,
				ProductId = model.ProductId,
				Amount = model.Amount,
				AssemblyType = (int)model.AssemblyType,
				AssemblySemiproducts = model.AssemblySemiproducts.Select(md => new OptAssemblySemiproduct { PrintOrderSemiproductId = md.PrintOrderSemiproductId, NotchOrderId = md.NotchOrderId }).ToList()
			};

			using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
			{
				try
				{
					await _context.AddAsync(raw);
					var whJournalList = new List<WarehouseJournalModel>();
					if (raw.AssemblySemiproducts != null && raw.AssemblySemiproducts.Any())
					{
						var semiproducts = GetSemiproductsByProductId(raw.ProductId).ToList();
						semiproducts.ForEach(semiproduct =>
						whJournalList.Add(
							new WarehouseJournalModel
							{
								ActionDate = raw.Date,
								ActionTypeId = WhJournalActionType.Outcome,
								ActionExtendedTypeId = WhJournalActionExtendedType.AssemblyPart,
								Amount = raw.Amount,
								WarehouseTypeId = WarehouseType.SemiproductReady,
								ObjectTypeId = WhJournalObjectType.Semiproduct,
								ObjectId = semiproduct.SemiproductId
							}));

					}
					await _context.SaveChangesAsync();
					whJournalList.ForEach(x => x.ActionId = raw.AssemblyId.ToString());
					await _whJournalManager.AddRangeAsync(whJournalList, false);
					await _context.SaveChangesAsync();
					await transaction.CommitAsync();
				}
				catch
				{
					await transaction.RollbackAsync();
					throw;
				}

			}
			return raw.AssemblyId;
		}

		public async Task<IEnumerable<AssemblyModel>> GetAssembliesAsync()
		{
			var raw = await _context.OptAssembly
				.Include(ass => ass.Product)
					.ThenInclude(prod => prod.Country)
				.Include(ass => ass.Warehouse)
				.OrderByDescending(ass => ass.Date).ToListAsync();
			var list = _mapper.Map<IEnumerable<AssemblyModel>>(raw);
			return list;
		}

		public async Task<PrintOrderAssemblyInfo> GetAssemblyInfo(AssemblyModel model, int requiredamount)
		{
			var product = _context.OptProduct.Find(model.ProductId);
			var printOrdersAssemblyInfo = new PrintOrderAssemblyInfo
			{
				ProductSeriesId = product.ProductSeriesId.Value,
				ConsiderNotch = model.ConsiderNotch
			};

			if (_excludedProducts.Contains(model.ProductId))
			{
				printOrdersAssemblyInfo.SkipCheck = true;
				return printOrdersAssemblyInfo;
			}

			var commonState = await _semiproductWarehouseManager.CalcSemiproductWhjournalAsync(model.ProductId);
			var state = commonState.Value;
			KeyValuePair<string, IEnumerable<SemiproductCalcDetails>> stickersState = default;
			var rawSemiProductsList = GetSemiproductsByProductId(model.ProductId);
			var universalStickerIsUsed =
				rawSemiProductsList.Select(semiProd => semiProd.ProductId).Distinct().Count() > 1;

			var semiproductContent = _semiproductWarehouseManager.GetSemiproductIds(rawSemiProductsList, model.ProductId, universalStickerIsUsed);
			if (universalStickerIsUsed)
			{
				var stickersProductId = rawSemiProductsList.First(semiProd =>
				semiProd.SemiproductTypeId == (int)SemiProductType.Stickers)
					.ProductId;
				stickersState = await _semiproductWarehouseManager.CalcSemiproductWhjournalAsync(stickersProductId);
			}
			var semiproductsLeftAmount = GetPrintOrderLeftAmount(model);

			(printOrdersAssemblyInfo.IsEnoughBlocksAtWarehouse, printOrdersAssemblyInfo.IsEnoughBlocksInPrintOrderIncoming) = semiproductContent.BlockId.HasValue ? IsSemiproductEnough(state, semiproductsLeftAmount, requiredamount, semiproductContent.BlockId.Value) : (false, false);
			//eventually change to notch 
			var stateUsedForStickers = universalStickerIsUsed ? stickersState.Value : state;
			(printOrdersAssemblyInfo.IsEnoughStickersAtWarehouse, printOrdersAssemblyInfo.IsEnoughStickersInPrintOrderIncoming) = semiproductContent.StickersId.HasValue ? IsSemiproductEnough(stateUsedForStickers, semiproductsLeftAmount, requiredamount, semiproductContent.StickersId.Value) : (false, false);

			if (printOrdersAssemblyInfo.ConsiderNotch)
			{
				if (requiredamount > 0)
				{
					var notchOrderId = model.AssemblySemiproducts.First(asmProd => asmProd.NotchOrderId.HasValue).NotchOrderId.Value;
					printOrdersAssemblyInfo.IsEnoughStickersInNotchOrderIncoming
						 = IsStickersInNotchIncomingEnough(model, notchOrderId, semiproductContent.StickersId.Value, requiredamount);
				}
				else
					printOrdersAssemblyInfo.IsEnoughStickersInNotchOrderIncoming = true;
			}
			(printOrdersAssemblyInfo.IsEnoughCoversAtWarehouse, printOrdersAssemblyInfo.IsEnoughCoversInPrintOrderIncoming) = semiproductContent.CoversId.HasValue ? IsSemiproductEnough(state, semiproductsLeftAmount, requiredamount, semiproductContent.CoversId.Value) : (false, false);
			return printOrdersAssemblyInfo;
		}

		public bool ConsiderNotch(int assemblyId)
		{
			return assemblyId == 0;
		}

		public async Task<AssemblyModel> GetAssemblyAsync(long assemblyId)
		{
			OptAssembly assemblyFromDb = await _assemblyRepository.GetAssembly(assemblyId);
			if (assemblyFromDb == null)
				throw new UserException($"Сборка не найдена");

			var model = _mapper.Map<AssemblyModel>(assemblyFromDb);

			return model;
		}

		public async Task DeleteAsync(long id)
		{
			var assembly = await _context.OptAssembly
				  .Include(asmbl => asmbl.Warehouse)
				  .FirstOrDefaultAsync(asmbl => asmbl.AssemblyId == id);

			if (assembly?.Warehouse != null)
			{
				throw new UserException($"Невозможно удалить сборку, она выбрана в приход от {assembly?.Warehouse?.DateAdd}");
			}

			_context.Remove(assembly);
			await _whJournalManager.RemoveByAction(assembly);
			await _context.SaveChangesAsync();
		}

		public bool CheckSprings(int amount)
		{
			return true;
		}

		public async Task EditAsync(AssemblyModel model)
		{
			var assemblyFromDb = await _context
				.OptAssembly.Include(x => x.AssemblySemiproducts)
				.FirstOrDefaultAsync(x => x.AssemblyId == model.AssemblyId);

			if (assemblyFromDb == null)
				throw new UserException($"Сборка не найдена");

			CheckPrintOrdersIsAssembled(model, assemblyFromDb);

			assemblyFromDb.Amount = model.Amount;

			if (model.AssemblySemiproducts != null && model.AssemblySemiproducts.Any())
			{
				var modelPrintOrderSemiProductIds = model.AssemblySemiproducts.Select(assSemiP => assSemiP.PrintOrderSemiproductId);
				var dbPrintOrderSemiProductIds = assemblyFromDb.AssemblySemiproducts.Select(assSemiP => assSemiP.PrintOrderSemiproductId);

				var itemsToRemove = _mapper.Map<List<OptAssemblySemiproduct>>(assemblyFromDb.AssemblySemiproducts.Where(assSemiP => !modelPrintOrderSemiProductIds.Contains(assSemiP.PrintOrderSemiproductId)));
				var itemsToAdd = _mapper.Map<List<OptAssemblySemiproduct>>(model.AssemblySemiproducts.Where(assSemiP => !dbPrintOrderSemiProductIds.Contains(assSemiP.PrintOrderSemiproductId))).ToList();

				itemsToAdd.ForEach(x => { x.AssemblyId = model.AssemblyId; });

				var notchInAssembly = model.AssemblySemiproducts.FirstOrDefault(asm => asm.NotchOrderId.HasValue);
				if (notchInAssembly != null)
				{
					var notchInDb = assemblyFromDb.AssemblySemiproducts.First(asm => asm.NotchOrderId.HasValue);
					if (notchInDb.NotchOrderId != notchInAssembly.NotchOrderId)
					{
						_context.Remove(notchInDb);
						//notchInAssembly.Amount = model.Amount;
						notchInAssembly.AssemblyId = model.AssemblyId;

						_context.Add(notchInAssembly);
					}
				}
				if (itemsToRemove.Any())
				{
					_context.RemoveRange(itemsToRemove);
				}
				if (itemsToAdd.Any())
				{
					await _context.AddRangeAsync(itemsToAdd);
				}

				await _whJournalManager.UpdateJournal(model);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<IEnumerable<ProductModel>> GetProductsForAssembly(AssemblyType assemblyType)
		{
			var rawQuery = _context.OptProduct
				.Include(product => product.Country)
				.Include(product => product.ProductSeries)
				.Where(product => product.ProductKindId == 1
				&& !Const.BoboProductIds.Contains(product.ProductId));

			switch (assemblyType)
			{
				case AssemblyType.Assembly:
					rawQuery = rawQuery.Where(product => Const.CalendarSeriedId != product.ProductSeriesId && !product.IsKit);
					break;

				case AssemblyType.Wind:
					rawQuery = rawQuery.Where(product => Const.CalendarSeriedId == product.ProductSeriesId);
					break;
			}

			var raw = await rawQuery
				.OrderBy(product => product.ProductSeries.Order)
					.ThenBy(product => product.DisplayOrder)
					.ToListAsync();

			var list = _mapper.Map<IEnumerable<ProductModel>>(raw);

			return list;
		}

		public void SetAssemblySemiproducts(AssemblyModel model)
		{
			var printOrderSemiproductIds = model.AssemblySemiproducts.Select(aSemiprod => aSemiprod.PrintOrderSemiproductId);
			var printOrderSemiproduct = _context.OptPrintOrderSemiproduct
				.Include(printOrderS => printOrderS.SemiProduct).Where(printOrderS => printOrderSemiproductIds.Contains(printOrderS.PrintOrderSemiproductId));

			var notchOrderId = model.AssemblySemiproducts.FirstOrDefault(aSemiprod => aSemiprod.NotchOrderId.HasValue)?.NotchOrderId;
			var notchOrder = notchOrderId.HasValue ? _context.OptNotchOrder.FirstOrDefault(nOrder => nOrder.NotchOrderId == notchOrderId.Value) : null;

			model.AssemblySemiproducts = model.AssemblySemiproducts
				.Select(assemblySemiproduct =>
			{
				if (assemblySemiproduct.PrintOrderSemiproductId.HasValue)
					assemblySemiproduct.PrintOrderSemiproduct = _mapper.Map<PrintOrderSemiproductModel>(printOrderSemiproduct.FirstOrDefault(item => item.PrintOrderSemiproductId == assemblySemiproduct.PrintOrderSemiproductId));

				if (assemblySemiproduct.NotchOrderId.HasValue)
				{
					assemblySemiproduct.NotchOrder = _mapper.Map<NotchOrderModel>(notchOrder);
				}
				return assemblySemiproduct;
			}).OrderBy(asm => Util.GetOrderBySemiproductType((int)asm.SemiproductTypeId)).ToList();
		}

		public async Task<Dictionary<SemiproductModel, IEnumerable<KeyValuePair<int, string>>>> GetSemiproductsAndOrdersAsync(int productId, bool considerNotch)
		{
			var semiproducts = GetSemiproductsByProductId(productId).ToList();
			var semiproductWithprintOrders = await _printOrderManager.GetPrintOrdersForSemiproductsAsync(semiproducts, considerNotch);
			//var state = (await _semiproductWarehouseManager.CalculateWhJournalSemiproducts())
			//	.SelectMany(semiprod => semiprod.SemiproductList);

			var result = new Dictionary<SemiproductModel, IEnumerable<KeyValuePair<int, string>>>();

			foreach (var smprod in semiproductWithprintOrders.OrderBy(smProd => Util.GetOrderBySemiproductType(smProd.Key.SemiproductTypeId)))
			{
				//var amount = state.FirstOrDefault(semiprod => semiprod.SemiproductId == smprod.Key.SemiproductId)
				//	?.CurrentAmount;
				//smprod.Key.LeftAmount = amount;

				result.Add(smprod.Key, new List<KeyValuePair<int, string>>(smprod.Value.Select(o => new KeyValuePair<int, string>(o.PrintOrderSemiproductId, o.DisplayName))));
			}
			if (considerNotch)
			{
				var stickers = semiproducts.FirstOrDefault(smp => smp.SemiproductTypeId == (int)SemiProductType.Stickers);
				if (stickers != null)
				{
					var notchOrders = await  _notchOrderManager.GetNotchOrdersByProductIdAsync(productId, stickers.SemiproductId);
					//var amount = state.FirstOrDefault(semiprod => semiprod.SemiproductId == stickers.SemiproductId)
					//?.CurrentAmount;

					var key = _mapper.Map<SemiproductModel>(stickers);
					//key.LeftAmount = amount;
					result.Add(key, notchOrders);
				}
			}

			return result;
		}

		private IDictionary<OptPrintOrderSemiproduct, int> GetPrintOrderLeftAmount(AssemblyModel model)
		{
			try
			{
				//найдем все полуфабрикаты из выбранных в текущую сборку заказов печати
				var printOrderSemiproducts = model.AssemblySemiproducts
					.Where(asm => asm.PrintOrderSemiproductId.HasValue)
				.Join(_context.OptPrintOrderSemiproduct, assemblySemiProd => assemblySemiProd.PrintOrderSemiproductId.Value,
				printOrderSemiprod => printOrderSemiprod.PrintOrderSemiproductId, (assemblyS, printOrderS) =>
				 printOrderS).ToList();

				//и их ид
				var printOrderSemiproductsIds = printOrderSemiproducts.Select(printOrderSemiprod => printOrderSemiprod.PrintOrderSemiproductId);

				// найдем полуфабрикаты из других сборок, с ид не равными текущей, в которых участвуют найденные выше полуфабрикаты 
				var anotherAssembliesSemiproducts = _context.OptAssemblySemiproduct
					.Include(assemblySemiprod => assemblySemiprod.Assembly)
					.Where(assemblySemiprod => assemblySemiprod.PrintOrderSemiproductId.HasValue && assemblySemiprod.AssemblyId != model.AssemblyId &&
						printOrderSemiproductsIds.Contains(assemblySemiprod.PrintOrderSemiproductId.Value))
					.ToList();

				//выясним, сколько было взято каждого полуфабриката 
				var takenAmountbySemiproduct = anotherAssembliesSemiproducts
					.GroupBy(assemblySemiprod => assemblySemiprod.PrintOrderSemiproductId, assemblySemiprod => assemblySemiprod.Assembly.Amount)
					.ToDictionary(group => group.Key, group => group.Sum());

				//выясним, сколько было в приходах каждого полуфабриката 
				var printOrdersIncoming =
					_context.OptPrintOrderIncomingItem.Where(item => printOrderSemiproductsIds.Contains(item.PrintOrderSemiproductId))
						.ToList()
					.GroupBy(incom => incom.PrintOrderSemiproductId, incom => incom.Amount)
					.ToDictionary(group => group.Key, group => group.Sum());

				var thisAssemblyAlreadyTaken = model.AssemblyId != 0 ?
				_context.OptAssembly.Find(model.AssemblyId).Amount : 0;
				var leftAmount = new Dictionary<OptPrintOrderSemiproduct, int>();
				printOrderSemiproducts.ForEach(printOrderSemiprod =>
				{
					takenAmountbySemiproduct.TryGetValue(printOrderSemiprod.PrintOrderSemiproductId, out int taken);
					printOrdersIncoming.TryGetValue(printOrderSemiprod.PrintOrderSemiproductId, out int incoming);
					leftAmount.Add(printOrderSemiprod, incoming - taken - thisAssemblyAlreadyTaken);
				});
				return leftAmount;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Assembly]");
				throw;
			}
		}

		private void CheckPrintOrdersIsAssembled(AssemblyModel model, OptAssembly assemblyFromDb = null)
		{
			var assembledPrintOrders = new List<string>();
			IEnumerable<AssemblySemiproductModel> assemblySemiproducts = model.AssemblySemiproducts;
			IEnumerable<int> assemblySemuproductsIds = Enumerable.Empty<int>();

			if (assemblyFromDb != null)
			{
				var existing = assemblyFromDb.AssemblySemiproducts
					.Where(assemblySemiproducts => assemblySemiproducts.PrintOrderSemiproductId.HasValue)
					.Select(aSemiproduct => aSemiproduct.PrintOrderSemiproductId.Value);

				assemblySemiproducts = assemblySemiproducts.Where(aSemiproduct =>
				aSemiproduct.PrintOrderSemiproductId.HasValue &&
				 !existing.Contains(aSemiproduct.PrintOrderSemiproductId.Value));
				assemblySemuproductsIds = assemblySemiproducts.Select(aSemiproduct => aSemiproduct.PrintOrderSemiproductId.Value).ToList();
			}
			_context.OptPrintOrderSemiproduct
			  .Include(printOrderSemiproduct => printOrderSemiproduct.PrintOrder)
				  .Where(printOrderSemiproduct => assemblySemuproductsIds.Contains(printOrderSemiproduct.PrintOrderSemiproductId))
				  .Select(printOrderSemiproduct => printOrderSemiproduct.PrintOrder)
				  .Where(printOrder => printOrder.IsAssembled.HasValue && printOrder.IsAssembled.Value)
				  .ToList()
				  .ForEach(printOrder =>
				  assembledPrintOrders.Add($"заказ печати {printOrder.OrderDate.Date.ToShortDateString()} {printOrder.PrintingHouseOrderNum} уже собран"));

			if (assembledPrintOrders.Any())
			{
				throw new UserException("При сохранении возникла ошибка: " + string.Join(", ", assembledPrintOrders));
			}
		}

		private (bool enoghAtWh, bool enoghInPrintOrderInc) IsSemiproductEnough(IEnumerable<SemiproductCalcDetails> semiproductWarehouseItems, IDictionary<OptPrintOrderSemiproduct, int> semiproductsPrintOrderLeftAmount, int amount, long semiproductId)
		{
			var semiproduct = semiproductWarehouseItems.FirstOrDefault(item => item.SemiproductId == semiproductId);
			if (semiproduct == null)
				return (false, false);
			var enoughAtWarehouse = semiproduct.Current >= amount;
			var printOrderSemiproduct = semiproductsPrintOrderLeftAmount.FirstOrDefault(dictItem => dictItem.Key.SemiproductId == semiproductId);
			var enoughInPrintOrderInc = printOrderSemiproduct.Value >= amount;
			return (enoughAtWarehouse, enoughInPrintOrderInc);
		}


		private bool IsStickersInNotchIncomingEnough(AssemblyModel model, int notchOrderId, int stickersId, int requiredamount)
		{
			try
			{
				// по notchOrderId найдем сумму всех приходов в этом заказе надсечки этих наклеек 
				var incoming = _context.OptNotchOrderIncoming
					.Where(nIcoming => nIcoming.NotchOrderId == notchOrderId)
					.Include(nIncoming => nIncoming.IncomingItems)
					.SelectMany(nIncoming => nIncoming.IncomingItems)
					.Where(nIcomingItem => nIcomingItem.SemiproductId == stickersId)
					.Sum(nIcomingItem => nIcomingItem.Amount);


				// найдем другие сборки с этим же заказом надсечки и этими же наклейками (=этой же тетрадью) 
				var taken = _context.OptAssemblySemiproduct
					.AsNoTracking()
					.Include(assemblySemiprod => assemblySemiprod.Assembly)
					.Where(assemblySemiprod => assemblySemiprod.NotchOrderId.HasValue &&
					 assemblySemiprod.NotchOrderId.Value == notchOrderId &&
					 assemblySemiprod.AssemblyId != model.AssemblyId &&
					  assemblySemiprod.Assembly.ProductId == model.ProductId)
					.Select(assemblySemiprod => assemblySemiprod.Assembly)
					.Sum(assembly => assembly.Amount);

				var leftAmount = incoming - taken;

				return leftAmount >= requiredamount;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Assembly]");
				throw;
			}
		}

		private IEnumerable<OptSemiproduct> GetSemiproductsByProductId(int productId)
		{
			var product = _productRepository.GetDetailsWithSemiproducts(productId);
			var list = product.Semiproducts.ToList();
			if (product.CountryId.HasValue && !list.Any(semiprod => semiprod.SemiproductTypeId == (int)SemiProductType.Stickers))
			{
				var universalStickers = _context.OptSemiproductProductRelation
					.Include(semiprodrel => semiprodrel.Semiproduct)
					.ThenInclude(semiprod => semiprod.SemiproductType)
				   .FirstOrDefault(semiprodrel => semiprodrel.ProductId == productId);
				if (universalStickers != null)
				{
					list.Add(universalStickers.Semiproduct);
				}
			}
			if (Const.CalendarSeriedId == product.ProductSeriesId)
			{
				list = list.Where(semiprod => semiprod.SemiproductTypeId == (int)SemiProductType.Block).ToList();
			}
			return list;
		}

	}
}