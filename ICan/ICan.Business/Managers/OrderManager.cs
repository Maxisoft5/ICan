using AutoMapper;
using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Common.Models.Opt.Report;
using ICan.Common.Repositories;
using ICan.Common.Utils;
using ICan.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class OrderManager : BaseManager
	{
		public readonly ProductManager _productManager;
		private readonly EventManager _eventManager;
		private readonly WarehouseJournalManager _whJournalManager;
		private readonly IOrderRepository  _orderRepository;
		private readonly IProductRepository _productRepository;
		private readonly IPriceRepository _priceRepository;
		private readonly UserManager<ApplicationUser> _userManager;

		private readonly string _imagePath;

		public OrderManager(IMapper mapper, ApplicationDbContext context,
			IProductRepository productRepository,
			IPriceRepository priceRepository,
			IOrderRepository orderRepository,
			ILogger<BaseManager> logger,
			UserManager<ApplicationUser> userManager,
			ProductManager productManager,
			EventManager eventManager,
			WarehouseJournalManager whJournalManager,
			IConfiguration configuration
			) : base(mapper, context, logger)
		{
			_imagePath = configuration["Settings:ImagePath"];
			_productManager = productManager;
			_eventManager = eventManager;
			_userManager = userManager;
			_whJournalManager = whJournalManager;
			_productRepository = productRepository;
			_priceRepository = priceRepository;
			_orderRepository = orderRepository;
		}

		public async Task<OptOrder> GetOrderAsync(Guid guid)
		{
			return await _context.OptOrder
				.Include(o => o.Client)
					.ThenInclude(client => client.ApplicationUserShopRelations)
					.ThenInclude(relation => relation.Shop)
				.Include(o => o.OrderStatus)
				.Include(o => o.OptOrderpayments)
				.Include(o => o.Photos)
				.Include(o => o.Requisites)
				.FirstOrDefaultAsync(m => m.OrderId == guid);
		}

		public async Task<OptOrder> CreateOrderAsync(string clientId,
			ClientOrderModel clientOrder)
		{
			IEnumerable<ShortOrderProductModel> items = clientOrder.OrderItems.Where(orderItem => orderItem.Amount > 0);

			var order = new OptOrder
			{
				ClientId = clientId,
				OrderDate = DateTime.Now,
				OrderStatusId = 1,
				TotalSum = 0,
				TotalWeight = 0,
				ClientAddress = clientOrder.Address,
				DeliveryPointAddress = clientOrder.PvzAddress
			};

			var optEvent = _eventManager.GetEventByDate(order.OrderDate);
			order.EventId = optEvent?.EventId;
			order.EventDiscountPercent = optEvent?.DiscountPercent;

			var client = await _userManager.FindByIdAsync(clientId);

			if (client.ClientType != (int)ClientType.Shop)
			{
				var requisites = _context.OptRequisites
									.Where(req => req.ClientType == client.ClientType)
									.OrderBy(req => req.LastUsed)
									.FirstOrDefault();
				requisites.LastUsed = DateTime.Now;
				order.RequisitesId = requisites?.RequisitesId;

			}
			var orderProducts = await SetOrderItemsList(order, items);

			_context.OptOrderproduct.AddRange(orderProducts);
			_context.OptOrder.Add(order);

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());
			}

			var finalOrder = _context.OptOrder.Find(order.OrderId);
			return order;
		}

		public async Task LogProductsToWhJournal(OrderModel orderModel)
		{
			var orderProducts = _context.OptOrderproduct.Where(orderP => orderP.OrderId == orderModel.OrderId).ToList();
			var journalItems = orderProducts
				.Select(oProduct => new WarehouseJournalModel
				{
					ActionDate = orderModel.AssemblyDate.Value,
					ActionTypeId = WhJournalActionType.Outcome,
					ActionExtendedTypeId = WhJournalActionExtendedType.Order,
					WarehouseTypeId = WarehouseType.NotebookReady,
					ActionId = orderModel.OrderId.ToString(),
					ObjectTypeId = WhJournalObjectType.Notebook,
					ObjectId = oProduct.ProductId,
					Amount = oProduct.Amount,
					Comment = $"Заказ №{orderModel.ShortOrderId}, дата сборки {orderModel.AssemblyDate?.ToShortDateString()}"
				});
			await _whJournalManager.AddRangeAsync(journalItems);
		}

		public IEnumerable<OrderModel> GetOrders(System.Security.Claims.ClaimsPrincipal user, bool showAll, string filterString, int pageSize, int offset, out int totalCount)
		{
			try
			{
				IQueryable<OptOrder> optOrders = _context.OptOrder
									.Include(o => o.Client)
									.Include(o => o.Shop)
									.Include(o => o.OrderStatus)
									.Include(o => o.OptOrderpayments)
									.Include(o => o.Requisites)
									.Where(t => showAll || t.ClientId == _userManager.GetUserId(user))
									.OrderByDescending(t => t.OrderDate);
				if (!string.IsNullOrWhiteSpace(filterString))
				{
					optOrders = Filter(optOrders, filterString);
				}
				totalCount = optOrders.Count();

				var list = optOrders.Skip(offset).Take(pageSize).ToList();
				var model = _mapper.Map<IEnumerable<OrderModel>>(list);
				return model;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при получении заказов");
				throw;
			}
		}

		public async Task<OrderModel> GetOrderModelAsync(Guid orderId, ApplicationUser user)
		{
			var optOrder = await GetOrderAsync(orderId);
			var onlyWithPrices = user.IsClient;
			if (optOrder == null)
			{
				return null;
			}

			if (optOrder.ClientId != user.Id && user.IsClient)
			{
				throw new UnauthorizedAccessException(Const.ErrorMessages.CantShowInfo);
			}
			var orderModel = _mapper.Map<OrderModel>(optOrder);
			await SetProducts(orderModel, optOrder, onlyWithPrices);
			SetPhotos(orderModel, optOrder);
			return orderModel;
		}

		public void SetPhotos(OrderModel orderModel, OptOrder optOrder)
		{
			orderModel.Photos = new List<OrderPhotoModel>();
			optOrder.Photos?.ToList().ForEach(photo =>
			{
				orderModel.Photos.Add(new OrderPhotoModel
				{
					OrderId = photo.OrderId,
					OrderPhotoId = photo.OrderPhotoId,
					PhotoDate = photo.PhotoDate,
					Photo = System.IO.File.ReadAllBytes(Path.Combine(_imagePath, photo.PhotoPath))
				});
			});
		}

		public async Task MapOrder(OrderModel orderModel, OptOrder optOrder)
		{
			optOrder.OrderStatusId = orderModel.OrderStatusId;
			optOrder.TrackNo = orderModel.TrackNo;
			optOrder.Comment = orderModel.Comment;
			optOrder.ClientAddress = orderModel.ClientAddress;
			optOrder.UpdNum = orderModel.UpdNum;
			optOrder.IsPaid = orderModel.IsPaid;
			optOrder.AssemblyDate = orderModel.AssemblyDate;
			optOrder.DoneDate = orderModel.DoneDate;
			optOrder.DeliveryPointAddress = orderModel.DeliveryPointAddress;
			optOrder.ShopId = orderModel.ShopId;

			await SetPersonalDiscount(orderModel, optOrder);

			if (orderModel.UploadOrderPhotos != null)
			{
				await UploadOrderPhotos(orderModel, optOrder);
			}
		}

		public async Task<int> GetShortOrderId(Guid guid)
		{
			return await _orderRepository.GetShortOrderId(guid);
		}

		public async Task<bool> UpdateOrder(OrderModel orderModel, OptOrder optOrder)
		{
			if (optOrder.OrderStatusId != orderModel.OrderStatusId /*только что поменяли*/)
			{
				await SetDatesAfterStatusChange(orderModel);
			}

			if (!string.IsNullOrWhiteSpace(orderModel.UpdNum) &&
				!orderModel.UpdNum.Equals(optOrder.UpdNum, StringComparison.InvariantCultureIgnoreCase))
			/*только что внесли номер УПД или заменили*/
			{
				ClearUpdinWarehouseJournal(orderModel.UpdNum, optOrder.Client.ApplicationUserShopRelations.Select(x => x.Shop?.ShopId));
			}

			var needChangeTrackNoNotify = (!string.Equals(optOrder.TrackNo?.Trim().ToLower(),
			  orderModel.TrackNo?.Trim().ToLower())
			   && !string.IsNullOrWhiteSpace(orderModel.TrackNo?.Trim().ToLower()));

			await MapOrder(orderModel, optOrder);

			_context.Update(optOrder);
			await _context.SaveChangesAsync();
			return needChangeTrackNoNotify;
		}

		private async Task ClearUpdinWarehouseJournal(string updNum, IEnumerable<int?> shopIds)
		{
			if (string.IsNullOrWhiteSpace(updNum))
				return;
			var minReportDate = DateTime.Now.AddDays(-90); //не будем рассматривать более старые упд, чтобы не взять упд с прошлого года, но смочь взять в январе упд из декабря прошлого года 
			var upd = _context.OptReport.FirstOrDefault(report => report.ReportKindId == (int)ReportKind.UPD
				   && report.ReportNum.Equals(updNum)
				   && report.ReportDate >= minReportDate);

			if (upd != null)
			{
				if (shopIds == null || !shopIds.Contains(upd.ShopId))
					throw new UserException("Невозможно сохранить номер УПД. Магазин в УПД и магазин клиента не совпадают");

				await _whJournalManager.RemoveByAction(upd.ReportId.ToString(), (int)WhJournalActionExtendedType.UPD);
			}
		}

		public async Task DeleteOrder(Guid guid)
		{
			var optOrder = await _context.OptOrder.FirstOrDefaultAsync(m => m.OrderId == guid);
			_context.OptOrder.Remove(optOrder);
			await _whJournalManager.RemoveByAction(optOrder.OrderId.ToString(), (int)WhJournalActionExtendedType.Order);
		}

		private async Task SetDatesAfterStatusChange(OrderModel orderModel)
		{
			var orderStatus = Enum.Parse<OrderStatus>(orderModel.OrderStatusId.ToString());
			await _whJournalManager.RemoveByAction(orderModel.OrderId.ToString(), (int)WhJournalActionExtendedType.Order);

			switch (orderStatus)
			{
				case OrderStatus.New:
					orderModel.DoneDate = null;
					orderModel.AssemblyDate = null;
					break;
				case OrderStatus.Assembling:
					orderModel.DoneDate = null;
					orderModel.AssemblyDate = DateTime.Now;
					await LogProductsToWhJournal(orderModel);
					break;

				case OrderStatus.Done:
					orderModel.AssemblyDate ??= DateTime.Now;
					orderModel.DoneDate = DateTime.Now;
					await LogProductsToWhJournal(orderModel);
					break;
			}
		}

		private async Task SetPersonalDiscount(OrderModel orderModel, OptOrder optOrder)
		{
			var personalDiscountChanged = optOrder.PersonalDiscountId != orderModel.PersonalDiscountId;
			optOrder.PersonalDiscountId = orderModel.PersonalDiscountId;

			if (personalDiscountChanged)
			{
				var personalDiscount = await _context.OptDiscount.FirstOrDefaultAsync(discount => discount.DiscountId == orderModel.PersonalDiscountId);
				optOrder.PersonalDiscountPercent = personalDiscount?.Value;

				await UpdateOrderWithPersonalDiscount(optOrder);
			}
		}

		private async Task UploadOrderPhotos(OrderModel orderModel, OptOrder optOrder)
		{
			var dirPath = Path.Combine(_imagePath, orderModel.OrderId.ToString());
			Directory.CreateDirectory(dirPath);
			foreach (var formFile in orderModel.UploadOrderPhotos)
			{
				if (formFile.Length > 0)
				{
					var filePath = Path.Combine(dirPath, formFile.FileName);
					var photoPath = Path.Combine(orderModel.OrderId.ToString(), formFile.FileName);
					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await formFile.CopyToAsync(stream);
						var orderPhoto = new OptOrderPhoto
						{
							OrderId = optOrder.OrderId,
							PhotoDate = DateTime.Now,
							PhotoPath = Path.Combine(photoPath)
						};
						_context.OptOrderPhotos.Add(orderPhoto);
					}
				}
			}
		}

		public SelectList GetDiscountList()
		{
			return new SelectList(_context.OptDiscount.Where(t => !t.IsArchived), "DiscountId", "Value");
		}

		public SelectList GetStatusList(int currentStatus)
		{
			return new SelectList(_context.OptOrderstatus, "OrderStatusId", "Name", currentStatus);
		}

		private async Task<IEnumerable<OptOrderproduct>> SetOrderItemsList(OptOrder order, IEnumerable<ShortOrderProductModel> items)
		{
			var productIds = items.Select(t => t.Id);
			var rawList = _productRepository.GetWithKitProducts(productIds);
			var products = _mapper.Map<IEnumerable<ProductModel>>(rawList).ToList();

			var orderProducts = new List<OptOrderproduct>();
			var totalNoteBookAmount = 0;
			var totalSum = 0.0;
			var totalWeight = 0.0;

			var user = await _userManager.FindByIdAsync(order.ClientId);

			foreach (var item in items)
			{
				var product = products.First(t => t.ProductId == item.Id);
				totalNoteBookAmount += product.ActingAmount;

				var orderProduct = new OptOrderproduct()
				{
					Order = order,
					ProductId = item.Id,
					Amount = item.Amount,
					DateAdd = order.OrderDate
				};
				orderProducts.Add(orderProduct);
			}

			// найти цены за ТУ дату
			var productPrices = _priceRepository.GetPrices(order.OrderDate)
				.Where(t => productIds.Contains(t.ProductId));

			var initialOrderPrices = items.Select(it =>
				  it.Amount * productPrices.First(prodprice => prodprice.ProductId == it.Id).Price);
			var initialOrderSum = initialOrderPrices.Sum();

			var orderSizeDiscount = _context.OptOrderSizeDiscount
				.FirstOrDefault(orderSize =>
					(int)orderSize.ClientType == user.ClientType &&
					orderSize.From <= initialOrderSum && initialOrderSum < orderSize.To && ((orderSize.DateStart <= order.OrderDate && order.OrderDate <= orderSize.DateEnd) || (orderSize.DateStart <= order.OrderDate && orderSize.DateEnd == null)))?.DiscountPercent ?? 0;

			order.OrderSizeDiscountPercent = orderSizeDiscount;
			var discountedPrices = new double[productIds.Count()];
			int current = 0;

			var totalDiscountPercent = (order.EventDiscountPercent ?? 0) + (order.PersonalDiscountPercent ?? 0) + (order.OrderSizeDiscountPercent ?? 0);

			foreach (var orderProduct in orderProducts)
			{
				var product = products.First(t => t.ProductId == orderProduct.ProductId);
				var actingAmount = product.ProductKindId == 1 /*Тетради*/
					? totalNoteBookAmount
					: orderProduct.Amount;

				var productPrice = productPrices.First(prodprice => prodprice.ProductId == orderProduct.ProductId);
				orderProduct.ProductPriceId = productPrice.ProductPriceId;

				totalWeight += orderProduct.Amount * product.Weight;
				var discountedPrice = (product.ProductKindId == Const.NoteBookProductKind
					? Math.Floor(productPrice.Price * (100 - totalDiscountPercent) / 100)
					: productPrice.Price);

				discountedPrices[current] = discountedPrice * orderProduct.Amount;
				current++;
				totalSum += productPrice.Price * orderProduct.Amount;
			}

			order.TotalSum = totalSum;
			order.DiscountedSum = discountedPrices.Sum();
			order.TotalWeight = Math.Round(totalWeight, 2);

			return orderProducts;
		}

		public async Task<string> UpdateOrder(Guid orderId, IEnumerable<ShortOrderProductModel> items)
		{
			var order = _context.OptOrder.Include(t => t.OptOrderproducts)
				.First(t => t.OrderId == orderId);
			var olds = _context.OptOrderproduct.Where(t => t.OrderId == order.OrderId);
			bool somethingChanged = CheckChanged(items, olds);
			await olds.ForEachAsync(q => _context.OptOrderproduct.Remove(q));
			order.IsPaid = !somethingChanged && order.IsPaid;
			var orderProducts = await SetOrderItemsList(order, items);
			await _context.OptOrderproduct.AddRangeAsync(orderProducts);
			_context.OptOrder.Update(order);

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());
			}
			return order.OrderId.ToString();
		}

		public async Task<string> UpdateOrderWithPersonalDiscount(OptOrder order)
		{
			var olds = _context.OptOrderproduct.Where(t => t.OrderId == order.OrderId);
			var shortOrderProductsList = olds
				.Select(oprod => new ShortOrderProductModel { Id = oprod.ProductId, Amount = oprod.Amount }).ToList();
			await olds.ForEachAsync(q => _context.OptOrderproduct.Remove(q));

			var orderProducts = await SetOrderItemsList(order, shortOrderProductsList);
			await _context.OptOrderproduct.AddRangeAsync(orderProducts);
			_context.OptOrder.Update(order);
			try
			{
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());
			}
			return order.OrderId.ToString();
		}

		public async Task SetProducts(OrderModel orderModel, OptOrder optOrder, bool onlyWithPrices)
		{
			var orderProducts = await _context.OptOrderproduct
				.Include(order => order.Product)
					.ThenInclude(product => product.KitProducts)
				.Include(order => order.Product)
					.ThenInclude(product => product.Country)
				.Include(order => order.ProductPrice)
				.Where(order => order.OrderId == optOrder.OrderId)
				.ToListAsync();

			var client = optOrder.Client;
			var productList = await _productManager.GetProducts(onlyWithPrices, optOrder.OrderId, optOrder.Client, dontShowDisabled: false);
			var productListPlain = productList.ProductGroups.SelectMany(productGroup => productGroup.Value);

			SetProductList(orderModel, productList);
			var products = (from product in orderProducts.ToList()
							join s in _context.OptProductseries.ToList() on product.Product.ProductSeriesId equals s.ProductSeriesId into se
							from s in se.DefaultIfEmpty()
							orderby s?.Order descending, product.ProductId
							select new { product, s }).ToList()
					  .OrderBy(t => t.s?.Order ?? int.MaxValue)
					  .ThenBy(t => (t.product.Product.DisplayOrder ?? 0));


			orderModel.OrderProducts = products.Select(orderProduct => new OrderProductModel
			{
				Productid = orderProduct.product.ProductId,
				Product = productListPlain.First(pr => pr.ProductId == orderProduct.product.ProductId).DisplayName,
				Amount = orderProduct.product.Amount,
				ProductPriceId = orderProduct.product.ProductPrice.ProductPriceId,
				Price = Convert.ToDouble(orderProduct.product.ProductPrice.Price),
				DiscountedPrice = Math.Floor(orderProduct.product.ProductPrice.Price * (100 - orderModel.TotalDiscountedPercent) / 100),
				Weight = orderProduct.product.Product.Weight,
				ProductKindId = orderProduct.product.Product.ProductKindId,
				IsKit = orderProduct.product.Product.IsKit,
				KitProducts = _mapper.Map<IEnumerable<KitProductModel>>(orderProduct.product.Product.KitProducts)
			}).ToList();

			orderModel.ProductList.ActiveEvent = await _eventManager.Get(optOrder.EventId);
			_productManager.SetMinOrderSize(orderModel.ProductList, client.ClientType, orderModel.OrderId);
		}

		public async Task<byte[]> Export(Guid orderId, ApplicationUser user)
		{
			var order = await GetOrderModelAsync(orderId, user);
			byte[] bytes = new byte[0];
			
			using (ExcelPackage objExcelPackage = new ExcelPackage())
			{
				ExcelWorksheet objWorksheet = objExcelPackage.Workbook.Worksheets.Add("Заказ");

				var current = 1;
				var shortOrderId = order.ShortOrderId > 0 ? order.ShortOrderId.ToString() : Util.GetShortNum(order.OrderId);
				objWorksheet.Cells[current, 1].Style.Font.Bold = true;
				objWorksheet.Cells[current, 1].Value = "Заказ";
				objWorksheet.Cells[++current, 1].Value = "Номер";
				objWorksheet.Cells[current, 2].Value = shortOrderId;
				objWorksheet.Cells[++current, 1].Value = "Дата заказа";
				objWorksheet.Cells[current, 2].Style.Numberformat.Format
					   = "dd.mm.yyyy";
				objWorksheet.Cells[current, 2].Value = order.OrderDate;

				objWorksheet.Cells[++current, 1].Value = "Статус";
				objWorksheet.Cells[current, 2].Value = order.OrderStatus;
				objWorksheet.Cells[++current, 1].Value = "Трекномер";
				objWorksheet.Cells[current, 2].Value = order.TrackNo;
				objWorksheet.Cells[++current, 1].Value = "Вес";
				objWorksheet.Cells[current, 2].Value = order.TotalWeight;

				objWorksheet.Cells[++current, 1].Value = "Общая сумма";
				objWorksheet.Cells[current, 2].Style.Numberformat.Format
					   = "#,##0.00";
				objWorksheet.Cells[current, 2].Value = order.TotalSum;

				if (order.EventDiscountPercent.HasValue && order.EventDiscountPercent.Value > 0)
				{
					objWorksheet.Cells[++current, 1].Value = "Скидка по акции, %";
					objWorksheet.Cells[current, 2].Style.Numberformat.Format = "0%";
					objWorksheet.Cells[current, 2].Value = Math.Round(order.EventDiscountPercent.Value / 100, 2);
				}

				if (order.PersonalDiscountPercent.HasValue && order.PersonalDiscountPercent.Value > 0)
				{
					objWorksheet.Cells[++current, 1].Value = "Скидка индивидуальная , %";
					objWorksheet.Cells[current, 2].Style.Numberformat.Format = "0%";
					objWorksheet.Cells[current, 2].Value = Math.Round(order.PersonalDiscountPercent.Value / 100, 2);
				}

				objWorksheet.Cells[++current, 1].Value = "Скидка в зависимости от суммы заказа, %";
				objWorksheet.Cells[current, 2].Style.Numberformat.Format = "0%";
				objWorksheet.Cells[current, 2].Value = Math.Round((order.OrderSizeDiscountPercent ?? 0) / 100, 2);

				objWorksheet.Cells[++current, 1].Value = "Общая скидка, %";
				objWorksheet.Cells[current, 2].Style.Numberformat.Format = "0%";
				objWorksheet.Cells[current, 2].Value = Math.Round(order.TotalDiscountedPercent / 100, 2);

				objWorksheet.Cells[++current, 1].Value = "Сумма с учётом скидки";
				objWorksheet.Cells[current, 2].Style.Numberformat.Format
					   = "#,##0.00";
				objWorksheet.Cells[current, 2].Value = order.DiscountedSum;

				objWorksheet.Cells[++current, 1].Value = "Клиент";
				objWorksheet.Cells[current, 2].Value = order.Client?.FullName;

				objWorksheet.Cells[++current, 1].Value = "Мобильный телефон";
				objWorksheet.Cells[current, 2].Value = order.Client?.PhoneNumber;
				
				objWorksheet.Cells[++current, 1].Value = "Адрес доставки";
				objWorksheet.Cells[current, 2].Value = order.ClientAddress;
				
				objWorksheet.Cells[++current, 1].Value = "Комментарий";
				objWorksheet.Cells[current, 2].Value = order.Comment;

				++current;
				objWorksheet.Cells[++current, 1].Style.Font.Bold = true;
				objWorksheet.Cells[current, 1].Value = "Товары в заказе";
				current++;
				objWorksheet.Cells[current, 1, current, 2].Style.Font.Bold = true;
				objWorksheet.Cells[current, 1, current, 2].Merge = true;
				objWorksheet.Cells[current, 1, current, 2].Style.WrapText = true;
				objWorksheet.Cells[current, 1, current, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

				objWorksheet.Cells[current, 1, current, 2].Value = "Наименование товара";
				objWorksheet.Cells[current, 3].Style.Font.Bold = true;
				objWorksheet.Cells[current, 3].Style.WrapText = true;

				objWorksheet.Cells[current, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

				objWorksheet.Cells[current, 3].Value = "Количество товара";
				objWorksheet.Cells[current, 4].Style.Font.Bold = true;
				objWorksheet.Cells[current, 4].Style.WrapText = true;
				objWorksheet.Cells[current, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

				objWorksheet.Cells[current, 4].Value = "Цена за единицу";
				objWorksheet.Cells[current, 5].Style.Font.Bold = true;
				objWorksheet.Cells[current, 5].Style.WrapText = true;
				objWorksheet.Cells[current, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

				objWorksheet.Cells[current, 5].Value = "Сумма";
			 
				foreach (var product in order.OrderProducts)
				{
					current++;
					//objWorksheet.Cells[current, 1].Value = product.Product;
					objWorksheet.Cells[current, 1, current, 2].Merge = true;
					objWorksheet.Cells[current, 1, current, 2].Value = product.Product;

					objWorksheet.Cells[current, 3].Value = product.Amount;
					objWorksheet.Cells[current, 4].Style.Numberformat.Format
					   = "#,##0.00";

					objWorksheet.Cells[current, 4].Value = product.DiscountedPrice;
					objWorksheet.Cells[current, 5].Style.Numberformat.Format
					   = "#,##0.00";
					objWorksheet.Cells[current, 5].Value = product.DiscountedPrice * product.Amount;
				}

				current += 2;
				objWorksheet.Cells[current, 1].Style.Font.Bold = true;
				objWorksheet.Cells[current, 1].Value = "Поступившие платежи";

				current++;
				objWorksheet.Cells[current, 1].Style.Font.Bold = true;
				objWorksheet.Cells[current, 1].Style.WrapText = true;
				objWorksheet.Cells[current, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

				objWorksheet.Cells[current, 1].Value = "Дата платежа";
				objWorksheet.Cells[current, 2].Style.Font.Bold = true;
				objWorksheet.Cells[current, 2].Style.Font.Bold = true;
				objWorksheet.Cells[current, 2].Style.WrapText = true;
				objWorksheet.Cells[current, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

				objWorksheet.Cells[current, 2].Value = "Сумма";

				foreach (var payment in order.OrderPayments)
				{
					current++;
					objWorksheet.Cells[current, 1].Style.Numberformat.Format
						   = "dd.mm.yyyy";
					objWorksheet.Cells[current, 1].Value = payment.OrderPaymentDate;
					objWorksheet.Cells[current, 2].Style.Numberformat.Format
				   = "#,##0.00";
					objWorksheet.Cells[current, 2].Value = payment.Amount;


				}
				//objWorksheet.Cells.AutoFitColumns(5.0, 25.0);
				objWorksheet.Column(1).Width = 25;
				objWorksheet.Column(2).Width = 25;
				objWorksheet.Column(3).Width = 12;
				objWorksheet.Column(4).Width = 10;
				objWorksheet.Column(5).Width = 10;

				//	objWorksheet.Cells.Style.Font.SetFromFont(new Font("Calibri", 10));
				//objWorksheet.Column(0).AutoFit();
				bytes = objExcelPackage.GetAsByteArray();
			}
			return bytes;
		}

		public async Task AddPaymentAsync(OldPaymentModel model)
		{
			var optPayment = new OptOrderpayment
			{
				OrderId = model.OrderId,
				OrderPaymentDate = model.OrderPaymentDate,
				Amount = model.Amount
			};
			await _context.OptOrderpayment.AddAsync(optPayment);
			await _context.SaveChangesAsync();
		}

		public async Task<Guid?> DeletePaymentAsync(int id)
		{
			var payment = _context.OptOrderpayment.First(t => t.OrderPaymentId == id);
			if (payment != null)
			{
				var orderId = payment.OrderId;
				_context.OptOrderpayment.Remove(payment);
				await _context.SaveChangesAsync();
				return orderId;
			}
			return null;
		}

		public async Task DeletePhotoAsync(int photoId)
		{
			var photo = _context.OptOrderPhotos.FirstOrDefault(t => t.OrderPhotoId == photoId);
			if (photo == null)
				return;
			_context.OptOrderPhotos.Remove(photo);
			await _context.SaveChangesAsync();
		}

		public bool OrderExists(string id)
		{
			if (!Guid.TryParse(id, out var guid))
				return false;
			return _context.OptOrder.Any(e => e.OrderId == guid);
		}

		private void SetProductList(OrderModel orderModel, ProductListModel productList)
		{
			orderModel.ProductList = productList;
			var productGroups = new Dictionary<OptProductseries, List<ProductModel>>();

			foreach (var group in productList.ProductGroups)
			{
				var productGroup = new KeyValuePair<OptProductseries, List<ProductModel>>(group.Key, new List<ProductModel>());

				foreach (var item in group.Value)
				{
					if (item.Amount > 0 || item.Enabled)
						productGroup.Value.Add(item);
				}
				if (productGroup.Value.Any() || group.Value.Any(t => t.Enabled))
					productGroups.Add(productGroup.Key, productGroup.Value);
			}
			orderModel.ProductList.ProductGroups = productGroups;
		}

		private IQueryable<OptOrder> Filter(IQueryable<OptOrder> optOrders, string filterString)
		{
			var filter = JsonConvert.DeserializeObject<OrderFilter>(filterString);

			if (!string.IsNullOrWhiteSpace(filter.ShortOrderDisplayId))
			{
				optOrders = optOrders.Where(ord => ord.ShortOrderId.ToString().Contains(filter.ShortOrderDisplayId));
			}
		
			if (!string.IsNullOrWhiteSpace(filter.Client))
			{
				var client = filter.Client.Trim().ToLower();
				optOrders = optOrders.Where(ord =>
					!string.IsNullOrWhiteSpace(ord.Client.LastName) && ord.Client.LastName.ToLower().Contains(client) ||
					!string.IsNullOrWhiteSpace(ord.Client.FirstName) && ord.Client.FirstName.ToLower().Contains(client));
			}
			if (!string.IsNullOrWhiteSpace(filter.ClientTypeName))
			{
				var clientType = filter.ClientTypeName.GetClientTypeByName();
				optOrders = optOrders.Where(ord =>
					 ord.Client.ClientType == (int)clientType);
			}
			if (!string.IsNullOrWhiteSpace(filter.ClientPhoneNumber))
			{
				optOrders = optOrders.Where(ord =>
					 !string.IsNullOrWhiteSpace(ord.Client.PhoneNumber)
					&& ord.Client.PhoneNumber.Contains(filter.ClientPhoneNumber));
			}
			if (!string.IsNullOrWhiteSpace(filter.OrderStatus))
			{
				optOrders = optOrders.Where(ord =>
					 ord.OrderStatus.Name == filter.OrderStatus);
			}
			if (filter.IsPaid.HasValue)
			{
				optOrders = optOrders.Where(ord =>
					 ord.IsPaid == filter.IsPaid);
			}
			if (!string.IsNullOrWhiteSpace(filter.PersonalDiscountPercent))
			{
				var percent = float.Parse(filter.PersonalDiscountPercent);
				optOrders = optOrders.Where(ord =>
					 ord.PersonalDiscountPercent == percent);

				optOrders = optOrders.Where(ord =>
					 ord.Requisites.Owner == filter.RequisitesOwner);
			}		
			if (!string.IsNullOrWhiteSpace(filter.Comment))
			{
				var pattern = filter.Comment.ToLower().Trim();
				optOrders = optOrders.Where(ord =>
					 !string.IsNullOrWhiteSpace(ord.Comment) && ord.Comment.ToLower().Trim().Contains(pattern));

				optOrders = optOrders.Where(ord =>
					 ord.Requisites.Owner == filter.RequisitesOwner);
			}
			if (!string.IsNullOrWhiteSpace(filter.OrderDateDisplay))
			{
				optOrders = optOrders.ToList().Where(ord => ord.OrderDate.ToShortDateString().Contains(filter.OrderDateDisplay)).AsQueryable();
			}
			return optOrders;
		}

		private static bool CheckChanged(IEnumerable<ShortOrderProductModel> items, IQueryable<OptOrderproduct> olds)
		{
			bool somethingChanged = false;
			if (olds.Count() != items.Count())
				somethingChanged = true;
			else
				foreach (var old in olds)
				{
					var cur = items.FirstOrDefault(t => t.Id == old.ProductId);
					if ((cur == null) || (old.Amount != cur.Amount))
					{
						somethingChanged = true;
						break;
					}
				};
			return somethingChanged;
		}
	}
}
