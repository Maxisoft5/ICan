using AutoMapper;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ICan.Common.Models.Opt;
using ICan.Data.Context;
using ICan.Common;
using ICan.Common.Repositories;

namespace ICan.Business.Managers
{
    public class NotchOrderManager : BaseManager
    {
        private readonly WarehouseJournalManager _whJournalManager;
        private readonly IPrintOrderRepository _printOrderRepository;
        private readonly INotchOrderRepository _notchOrderRepository;

        public NotchOrderManager(IMapper mapper, ApplicationDbContext context, ILogger<BaseManager> logger,
            WarehouseJournalManager whJournalManager, IPrintOrderRepository printOrderRepository,
            INotchOrderRepository notchOrderRepository) : base(mapper, context, logger)
        {
            _whJournalManager = whJournalManager;
            _printOrderRepository = printOrderRepository;
            _notchOrderRepository = notchOrderRepository;
        }

        public async Task<IEnumerable<NotchOrderModel>> GetOrders()
        {
            var orders = await _context.OptNotchOrder
                .Include(notchOrder => notchOrder.NotchOrderItems)
                    .ThenInclude(notchOrderItem => notchOrderItem.PrintOrder)
                    .ThenInclude(printOrders => printOrders.PrintOrderSemiproducts)
                    .ThenInclude(printOrdersSemiprod => printOrdersSemiprod.SemiProduct)
                    .ThenInclude(semipod => semipod.SemiproductType)
                .Include(notchOrder => notchOrder.NotchOrderItems)
                    .ThenInclude(notchOrderItem => notchOrderItem.PrintOrder)
                    .ThenInclude(printOrders => printOrders.PrintOrderSemiproducts)
                    .ThenInclude(printOrdersSemiprod => printOrdersSemiprod.SemiProduct)
                    .ThenInclude(semipod => semipod.Product)
                    .ThenInclude(product => product.ProductSeries)
                .Include(notchOrder => notchOrder.NotchOrderItems)
                    .ThenInclude(notchOrderItem => notchOrderItem.PrintOrder)
                    .ThenInclude(printOrders => printOrders.PrintOrderSemiproducts)
                    .ThenInclude(printOrdersSemiprod => printOrdersSemiprod.SemiProduct)
                    .ThenInclude(semipod => semipod.Product)
                .Include(notchOrder => notchOrder.NotchOrderStickers)
                    .ThenInclude(printOrdersSemiprod => printOrdersSemiprod.Semiproduct)
                    .ThenInclude(semipod => semipod.SemiproductType)
                .Include(notchOrder => notchOrder.NotchOrderStickers)
                    .ThenInclude(printOrdersSemiprod => printOrdersSemiprod.Semiproduct)
                    .ThenInclude(semipod => semipod.Product)
                    .ThenInclude(product => product.ProductSeries)
                .Include(notchOrder => notchOrder.NotchOrderStickers)
                    .ThenInclude(printOrdersSemiprod => printOrdersSemiprod.Semiproduct)
                    .ThenInclude(semipod => semipod.Product)
                    .ThenInclude(product => product.Country)
                .OrderByDescending(notchOrder => notchOrder.OrderDate)
                .ToListAsync();
            return _mapper.Map<IEnumerable<NotchOrderModel>>(orders);
        }

        public async Task Create(NotchOrderModel notchOrder)
        {
            if (notchOrder.NotchOrderItems == null || !notchOrder.NotchOrderItems.Any())
                throw new UserException("Невозможно сохранить заказ надсечки без наклеек!");
            using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var printOrderIds = notchOrder.NotchOrderItems.Select(notchItem => notchItem.PrintOrderId);
                    List<int> stickers = GetStickers(printOrderIds);
                    notchOrder.NotchOrderStickers = new List<NotchOrderStickerModel>();
                    stickers.ForEach(sticker => notchOrder.NotchOrderStickers.Add(new NotchOrderStickerModel { SemiproductId = sticker }));
                    var order = _mapper.Map<OptNotchOrder>(notchOrder);
                    await _notchOrderRepository.Add(order);
                    await AddNewNotchToJournal(order);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, Const.ErrorMessages.CantSave);
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private List<int> GetStickers(IEnumerable<int> printOrderIds)
        {
            List<int> stickers = new List<int>();
            printOrderIds.ToList().ForEach(printOrderId =>
                stickers.AddRange(_printOrderRepository.GetSemiproductdById(printOrderId)));
            stickers = stickers.Distinct().ToList();
            return stickers;
        }

        public async Task<NotchOrderModel> GetById(int notchOrderId)
        {
            var raw = await _context.OptNotchOrder
                .Include(x => x.NotchOrderItems)
                    .ThenInclude(x => x.PrintOrder)
                .Include(x => x.NotchOrderIncomings)
                    .ThenInclude(x => x.IncomingItems)
                .Where(x => x.NotchOrderId == notchOrderId)
            .FirstAsync();
            var incomingIds = raw.NotchOrderIncomings.Select(notchOrderIncoming => notchOrderIncoming.NotchOrderIncomingId);
            var order = _mapper.Map<NotchOrderModel>(raw);

            var incomingItemsRaw = _context.OptNotchOrderIncomingItem
                        .Include(incomingItem => incomingItem.Semiproduct)
                            .ThenInclude(notchOrderItem => notchOrderItem.Product)
                                .ThenInclude(product => product.Country)
                        .Include(incomingItem => incomingItem.Semiproduct)
                            .ThenInclude(notchOrderItem => notchOrderItem.SemiproductType)
                        .Where(notchOrderIncoming => incomingIds.Contains(notchOrderIncoming.NotchOrderIncomingId))
                        .ToList();
            var incomingItems = _mapper.Map<IEnumerable<NotchOrderIncomingItemModel>>(incomingItemsRaw);
            order.NotchOrderIncomings.ForEach(incoming =>
            {
                incoming.IncomingItems = incomingItems.Where(inc => inc.NotchOrderIncomingId == incoming.NotchOrderIncomingId).ToList();
            });
            order.IsUsed = await _context.OptAssemblySemiproduct.AnyAsync(x => x.NotchOrderId == notchOrderId);
            return order;
        }

        public async Task Update(NotchOrderModel model)
        {
            if (model.NotchOrderItems == null || !model.NotchOrderItems.Any())
                throw new UserException("Невозможно сохранить заказ надсечки без наклеек!");
            using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var raw = await _context.OptNotchOrder
                    .Include(x => x.NotchOrderIncomings)
                        .ThenInclude(x => x.IncomingItems)
                    .Include(x => x.NotchOrderItems)
                    .Include(x => x.NotchOrderStickers)
                    .FirstAsync(x => x.NotchOrderId == model.NotchOrderId);
                    _context.Entry(raw).State = EntityState.Detached;

                    var order = _mapper.Map<OptNotchOrder>(model);
                    _context.Entry(order).State = EntityState.Modified;

                    await ChangeNotchOderItems(raw, model, order);
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, Const.ErrorMessages.CantSave);
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task Delete(int id)
        {
            var notchOrder = await _context.OptNotchOrder
                .Include(notch => notch.NotchOrderIncomings)
                .FirstAsync(x => x.NotchOrderId == id);

            await _whJournalManager.RemoveByAction(notchOrder.NotchOrderIncomings, false);
            await _whJournalManager.RemoveByAction(notchOrder.NotchOrderId.ToString(), (int)WhJournalActionExtendedType.NotchOrder, false);
            _context.Remove(notchOrder);

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PrintOrderModel>> GetPrintOrdersForNotch(int? currentNotchOrderId)
        {
            var printOrders = await _context.OptPrintOrder
                .Include(printOrder => printOrder.NotchOrderItem)
                .Include(printOrder => printOrder.PrintOrderSemiproducts)
                    .ThenInclude(printOrderIncoming => printOrderIncoming.PrintOrderIncomingItems)
                .Include(printOrder => printOrder.PrintOrderSemiproducts)
                    .ThenInclude(printOrderSemirproduct => printOrderSemirproduct.SemiProduct)
                .Where(printOrder => printOrder.PrintOrderSemiproducts != null &&
                                     printOrder.PrintOrderSemiproducts.Any()
                                    && printOrder.PrintOrderSemiproducts.First().SemiProduct.SemiproductTypeId == (int)SemiProductType.Stickers
                                    && printOrder.PrintOrderSemiproducts
                                        .Any(sProd => !sProd.IsAssembled)
                                     && (printOrder.NotchOrderItem == null
                                     || currentNotchOrderId.HasValue && printOrder.NotchOrderItem.NotchOrderId == currentNotchOrderId.Value))
                .Where(printOrder => printOrder.IsArchived == false)
                .ToListAsync();
            var ordersWitnIncome = printOrders.Where(printOrder => printOrder.PrintOrderSemiproducts.All(printOrderSemiproduct =>
                     printOrderSemiproduct.PrintOrderIncomingItems.Sum(incomingItem => incomingItem.Amount) >= printOrder.Printing))
                .ToList();
            var result = _mapper.Map<IEnumerable<PrintOrderModel>>(ordersWitnIncome);
            return result;
        }

        public async Task<List<NotchOrderIncomingItemModel>> GetNotchOrderIncomingsByNotchOrderId(int notchOrderId)
        {
            try
            {
                var notchStickers = await _context.OptNotchOrderSticker
                 .Where(notchOrderS => notchOrderS.NotchOrderId == notchOrderId)
                            .Include(notchOrderS => notchOrderS.Semiproduct)
                                .ThenInclude(semiproduct => semiproduct.Product)
                                    .ThenInclude(product => product.Country)
                            .Include(notchOrderS => notchOrderS.Semiproduct)
                                .ThenInclude(semiproduct => semiproduct.SemiproductType)
                                .ToListAsync();
                var list = notchStickers
                    .Select(notchSticker =>
                        new NotchOrderIncomingItemModel
                        {
                            Semiproduct = _mapper.Map<SemiproductModel>(notchSticker.Semiproduct),
                            SemiproductId = notchSticker.SemiproductId,
                            NotchOrderStickerId = notchSticker.NotchOrderStickerId
                        }
                    ).ToList();
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[NotchOrder][AddIncoming]");
                throw;
            }
        }

        public async Task AddIncoming(NotchOrderIncomingModel incomingModel)
        {
            if (incomingModel.IncomingItems.Any(imcomingItem => imcomingItem.Amount > 0))
            {
                using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var incoming = new OptNotchOrderIncoming
                        {
                            IncomingDate = incomingModel.IncomingDate,
                            NotchOrderId = incomingModel.NotchOrderId
                        };
                        await _context.AddAsync(incoming);
                        await _context.SaveChangesAsync();
                        var date = DateTime.Now;
                        foreach (var item in incomingModel.IncomingItems.Where(x => x.Amount > 0))
                        {
                            var incomingItem = _mapper.Map<OptNotchOrderIncomingItem>(item);
                            incomingItem.NotchOrderIncoming = incoming;
                            await _context.AddAsync(incomingItem);
                            await AddIncomingToJournal(incoming.NotchOrderIncomingId.ToString(), date, item);
                        }

                        await _context.SaveChangesAsync();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, Const.ErrorMessages.CantSave);
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task DeleteIncoming(int id)
        {
            var incoming = await _context.OptNotchOrderIncoming.FirstAsync(x => x.NotchOrderIncomingId == id);

            //  delete income to semiproduct ready warehousetype for selected notchorder income
            //  delete outcome from stickersNotching warehousetype for selected notchorder income
            // so delete at once all journal entries with noth order incoming id 
            await _whJournalManager.RemoveByAction(new List<OptNotchOrderIncoming> { incoming }, false);
            _context.Remove(incoming);
            await _context.SaveChangesAsync();
        }

        private async Task AddIncomingToJournal(string notchOrderIncomingId, DateTime date, NotchOrderIncomingItemModel item)
        {
            //  outcome from stickersNotching warehousetype for selected notchorder  income
            var outComeFromNothing = new WarehouseJournalModel
            {
                ActionDate = date,
                ObjectTypeId = WhJournalObjectType.Semiproduct,
                ObjectId = item.SemiproductId.Value, //!!
                ActionTypeId = WhJournalActionType.Outcome,
                ActionExtendedTypeId = WhJournalActionExtendedType.NotchOrderIncoming,
                WarehouseTypeId = WarehouseType.StickersNotching,
                ActionId = notchOrderIncomingId,
                Amount = item.Amount,
            };

            //  income to semiproduct ready warehousetype for selected  notchorder income
            var incomeToReady = new WarehouseJournalModel
            {
                ActionDate = date,
                ObjectTypeId = WhJournalObjectType.Semiproduct,
                ObjectId = item.SemiproductId.Value, //!!,
                ActionTypeId = WhJournalActionType.Income,
                ActionExtendedTypeId = WhJournalActionExtendedType.NotchOrderIncoming,
                WarehouseTypeId = WarehouseType.SemiproductReady,
                ActionId = notchOrderIncomingId,
                Amount = item.Amount,
            };

            await _whJournalManager.AddRangeAsync(new List<WarehouseJournalModel> { incomeToReady, outComeFromNothing });
        }

        public async Task<IEnumerable<KeyValuePair<int, string>>> GetNotchOrdersByProductIdAsync(int productId, int stickersId)
        {
            var notchOrders = await _context.OptNotchOrder
                .Include(nOrder => nOrder.NotchOrderItems)
                    .ThenInclude(nOrderItem => nOrderItem.PrintOrder)
                        .ThenInclude(nOrderItemPrintOrder => nOrderItemPrintOrder.PrintOrderSemiproducts)
                .Include(nOrder => nOrder.NotchOrderIncomings)
                    .ThenInclude(nOrderInc => nOrderInc.IncomingItems)
                .Where(nOrder =>
                  nOrder.NotchOrderIncomings.Any(incs => incs.IncomingItems.Any(x => x.SemiproductId == stickersId && x.IsAssembled))).OrderBy(notch => notch.OrderDate).ToListAsync();
            
            var list = _mapper.Map<List<NotchOrderModel>>(notchOrders);

            return list.Select(notch => new KeyValuePair<int, string>(notch.NotchOrderId, $"{notch.GetDisplayForAssembly()}")).ToList();
        }

        private async Task ChangeNotchOderItems(OptNotchOrder raw, NotchOrderModel model, OptNotchOrder order)
        {
            var existingInDbPrintOrderIds = raw.NotchOrderItems.Select(notchItem => notchItem.PrintOrderId);
            var existingInModelPrintOrderIds = model.NotchOrderItems.Select(notchItem => notchItem.PrintOrderId);

            var notchOrderItemToAddIds = model
                    .NotchOrderItems
                    .Where(item => !existingInDbPrintOrderIds.Contains(item.PrintOrderId));

            var notchOrderItemToDeleteIds = raw
                .NotchOrderItems.Where(item => !existingInModelPrintOrderIds.Contains(item.PrintOrderId))
                .Select(item => item.NotchOrderItemId);


            if (notchOrderItemToAddIds.Any() || notchOrderItemToDeleteIds.Any())
            {
                if (raw.NotchOrderIncomings != null && raw.NotchOrderIncomings.Any())
                    throw new UserException("Невозможно изменить заказ надсечки. Сначала удалите приходы!");

                await _whJournalManager.RemoveByAction(order.NotchOrderId.ToString(), (int)WhJournalActionExtendedType.NotchOrder);
                order.NotchOrderItems = null;
                order.NotchOrderStickers = new List<OptNotchOrderSticker>();

                var rawStickersToRemove = _context.OptNotchOrderSticker
                    .Where(sticker => sticker.NotchOrderId == raw.NotchOrderId);

                var rawItemsToRemove = _context.OptNotchOrderItem.Where(item => item.NotchOrderId == raw.NotchOrderId);
                _context.RemoveRange(rawItemsToRemove);
                var rawItemsToAdd = _mapper.Map<IEnumerable<OptNotchOrderItem>>(model.NotchOrderItems.Select(item => { item.NotchOrderId = order.NotchOrderId; return item; }));
                _context.AddRange(rawItemsToAdd);
                var stickersToRemove = _context.OptNotchOrderSticker
                    .Where(sticker => sticker.NotchOrderId == model.NotchOrderId);
                _context.RemoveRange(stickersToRemove);

                var addedStickers = GetStickers(model.NotchOrderItems.Select(s => s.PrintOrderId));
                addedStickers.ForEach(sticker => order.NotchOrderStickers.Add(new OptNotchOrderSticker { NotchOrderId = order.NotchOrderId, SemiproductId = sticker }));

                await AddNewNotchToJournal(order);
            }

            order.NotchOrderIncomings = raw.NotchOrderIncomings;

            var modelIncomingsItem = model.NotchOrderIncomings.SelectMany(x => x.IncomingItems);
            var orderExistingIncomingItems = order.NotchOrderIncomings.SelectMany(x => x.IncomingItems);

            foreach (var incomingItem in orderExistingIncomingItems)
            {
                incomingItem.IsAssembled = modelIncomingsItem.FirstOrDefault(x => x.NotchOrderIncomingItemId == incomingItem.NotchOrderIncomingItemId).IsAssembled;
            }
        }

        private async Task AddNewNotchToJournal(OptNotchOrder notchOrder)
        {
            var date = DateTime.Now;
            var printOrderIds = notchOrder.NotchOrderItems.Select(notchItem => notchItem.PrintOrderId).ToList();
            var printOrders = _context.OptPrintOrder
                    .Include(printOrder => printOrder.PrintOrderSemiproducts)
                .Where(printOrder => printOrderIds.Contains(printOrder.PrintOrderId));

            foreach (var printOrder in printOrders)
            {
                foreach (var printOrderSemiproduct in printOrder.PrintOrderSemiproducts)
                {
                    // outcome from stickersUnNotched warehousetype
                    var outcomeFromUnNotched = new WarehouseJournalModel
                    {
                        ActionDate = date,
                        ObjectTypeId = WhJournalObjectType.Semiproduct,
                        ObjectId = printOrderSemiproduct.SemiproductId,
                        ActionTypeId = WhJournalActionType.Outcome,
                        ActionExtendedTypeId = WhJournalActionExtendedType.NotchOrder,
                        WarehouseTypeId = WarehouseType.StickersUnNotched,
                        ActionId = notchOrder.NotchOrderId.ToString(),
                        Amount = printOrder.Printing,
                    };

                    // income to stickersNotching warehousetype
                    var incomeToNotching = new WarehouseJournalModel
                    {
                        ActionDate = date,
                        ObjectTypeId = WhJournalObjectType.Semiproduct,
                        ObjectId = printOrderSemiproduct.SemiproductId,
                        ActionTypeId = WhJournalActionType.Income,
                        ActionExtendedTypeId = WhJournalActionExtendedType.NotchOrder,
                        WarehouseTypeId = WarehouseType.StickersNotching,
                        ActionId = notchOrder.NotchOrderId.ToString(),
                        Amount = printOrder.Printing
                    };
                    await _whJournalManager.AddRangeAsync(new List<WarehouseJournalModel> { incomeToNotching, outcomeFromUnNotched }, false);
                }
            }
        }
    }
}
