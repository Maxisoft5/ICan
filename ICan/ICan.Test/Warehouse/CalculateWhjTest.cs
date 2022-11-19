using AutoFixture;
using AutoMapper;
using ICan.Business.Managers;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Common.Domain;
using ICan.Test.Fakes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using ICan.Data.Context;
using ICan.Common.Models.Opt;
using ICan.Common;
using ICan.Data.Repositories;
using ICan.Business;

namespace ICan.Test.Warehouse
{
	public class CalculateWhj
	{
		private CalcManager _calcManager { get; set; }
		private SemiproductWarehouseManager _semiproductWarehouseManager { get; set; }
		private CommonManager<OptPaper> _commonManager { get; set; }
		private WarehouseManager _warehouseManager { get; set; }

		private ILogger<BaseManager> _logger { get; set; }
		private Mapper _mapper { get; set; }
		private EventManager _eventManager { get; set; }
		private ProductManager _productManager { get; set; }

		private PriceManager _priceManager;
		private WarehouseJournalManager _whjJournalManager;
		private SemiproductManager _semiproductManager;
		private OrderManager _orderManager;
		private GluePadWarehouseManager _gluePadManager;
		private UniversalWarehouseManager _universalManager;

		[Theory, MemberData(nameof(DataFor_GetProductAmount_Returning_ShouldIncreaseAmount))]
		public async Task GetProductAmount_Returning_ShouldIncreaseAmount(DateTime inventoryDate, DateTime returningDate, int inventoryAmount, int returnAmount, int expectedAmount)
		{
			//Arrange
			var fixture = GetFixture();
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
					.UseInMemoryDatabase(databaseName: DateTime.Now.Ticks.ToString())
					.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
					.Options;

			using (var context = new ApplicationDbContext(options))
			{
				InitManagers(context, fixture);
				await InitData(inventoryDate, inventoryAmount);

				//Act
				await _warehouseManager.Create(new WarehouseModel
				{
					WarehouseActionTypeId = (int)WarehouseActionType.Returning,
					DateAdd = returningDate,
					WarehouseId = 29,
					WarehouseTypeId = WarehouseType.NotebookReady,
					WarehouseItems = new List<WarehouseItemModel> { new WarehouseItemModel {
					 Amount =returnAmount,
					 ProductId =4
					}
				}
				});

				var (journal, singleInventory) = _calcManager.GetProductWarehouseState(4, inventoryDate, WarehouseType.NotebookReady);
				var caldWjhDetails = new CalcWhjDetails
				{
					ProductId = 4,
					Journal = journal,
					InventoryDate = inventoryDate,
					Inventory = inventoryAmount,
					SingleInventory = singleInventory?.WarehouseItems.First(whItem => whItem.ProductId == 4).Amount,
					SingleInventoryDate = singleInventory?.DateAdd
				};
				//Assert
				Assert.Equal(expectedAmount, caldWjhDetails.Current);
			}
		}

		public static IEnumerable<object[]> DataFor_GetProductAmount_Returning_ShouldIncreaseAmount =>
		   new List<object[]>
		   {
				new object[] { new DateTime(2020,1,10), new DateTime(2020, 1, 15), 5, 7, 12 },
				new object[] { new DateTime(2020,1,10), new DateTime(2020, 1, 05), 5, 7, 5 },
		   };


		[Theory, MemberData(nameof(DataFor_GetProductAmount_KitAssembly_ShouldChangeAssemblyAmount))]
		public async Task GetProductAmount_KitAssembly_ShouldChangeAssemblyAmount(DateTime inventoryDate, DateTime actionDate, int inventoryAmount, int productId, int assemblyAmount, int expectedAmount)
		{
			//Arrange
			var fixture = GetFixture();
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
					.UseInMemoryDatabase(databaseName: DateTime.Now.Ticks.ToString())
					.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
					.Options;

			using (var context = new ApplicationDbContext(options))
			{
				InitManagers(context, fixture);
				await InitKitAssemblyData(inventoryDate, inventoryAmount);

				//Act
				var whId = 29;
				await _warehouseManager.Create(new WarehouseModel
				{
					WarehouseId = whId,
					WarehouseActionTypeId = (int)WarehouseActionType.KitAssembly,
					DateAdd = actionDate,
					WarehouseTypeId = WarehouseType.NotebookReady,
					WarehouseItems = new List<WarehouseItemModel> { new WarehouseItemModel {
					 Amount =assemblyAmount,
					 ProductId = 3
					}
				}
				});

				var (journal, singleInventory) = _calcManager.GetProductWarehouseState(productId, inventoryDate, WarehouseType.NotebookReady);
				var caldWjhDetails = new CalcWhjDetails
				{
					ProductId = productId,
					Journal = journal,
					SingleInventory = singleInventory?.WarehouseItems.First(whItem => whItem.ProductId == productId).Amount,
					SingleInventoryDate = singleInventory?.DateAdd,
					InventoryDate = inventoryDate,
					Inventory = inventoryAmount
				};
				//Assert
				Assert.Equal(expectedAmount, caldWjhDetails.Current);
			}
		}

		public static IEnumerable<object[]> DataFor_GetProductAmount_KitAssembly_ShouldChangeAssemblyAmount =>
		   new List<object[]>
		   {
				new object[] { new DateTime(2020,1,10), new DateTime(2020, 1, 15), 10, 3 /*productId*/, 4, 14 },  // for kit 
				new object[] { new DateTime(2020,1,10), new DateTime(2020, 1, 05), 10, 3 /*productId*/, 7, 10 },
			   new object[] { new DateTime(2020,1,10), new DateTime(2020, 1, 15), 10, 4 /*productId*/, 4, 6 },// for kit  [art
				new object[] { new DateTime(2020,1,10), new DateTime(2020, 1, 05), 10, 4 /*productId*/, 7, 10 },
		   };

		[Theory, MemberData(nameof(DataFor_GetProductAmount_ChangeOrderStateAssembling_ShouldDecreaseAmount))]
		public async Task GetProductAmount_ChangeOrderStateToAssembly_ShouldDecreaseAmount(DateTime inventoryDate, int inventoryAmount, int productId, int orderAmount, int expectedAmount)
		{
			//Arrange
			var fixture = GetFixture();
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
					.UseInMemoryDatabase(databaseName: DateTime.Now.Ticks.ToString())
					.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
					.Options;

			using (var context = new ApplicationDbContext(options))
			{
				InitManagers(context, fixture);
				await InitKitAssemblyData(inventoryDate, inventoryAmount);
			
				var order = await InitOrder(productId, orderAmount);
				var orderId = order.OrderId;
				//Act
				await _orderManager.UpdateOrder(new OrderModel { OrderId = orderId, OrderStatusId = (int)OrderStatus.Assembling }, order);

				var (journal, singleInventory) = _calcManager.GetProductWarehouseState(productId, inventoryDate, WarehouseType.NotebookReady);
				var caldWjhDetails = new CalcWhjDetails
				{
					ProductId = productId,
					Journal = journal,
					InventoryDate = inventoryDate,
					Inventory = inventoryAmount,
					SingleInventory = singleInventory?.WarehouseItems.First(whItem => whItem.ProductId == productId).Amount,
					SingleInventoryDate = singleInventory?.DateAdd
				};

				//Assert
				Assert.Equal(expectedAmount, caldWjhDetails.Current);
			}
		}

		private async Task<OptOrder> InitOrder(int productId, int actingAmount)
		{
			var orderItems = new ShortOrderProductModel[] { new ShortOrderProductModel { Id = productId, Amount = actingAmount } };
			var orderModel = new ClientOrderModel
			{
				OrderItems = orderItems,
				Address = "Адрес клиента"
			};
			var optOrder = await _orderManager.CreateOrderAsync(Guid.NewGuid().ToString(), orderModel);
			return optOrder;
		}

		public static IEnumerable<object[]> DataFor_GetProductAmount_ChangeOrderStateAssembling_ShouldDecreaseAmount =>
		   new List<object[]>
		   {
			   //(DateTime inventoryDate,  int inventoryAmount, int  productId, int orderAmount, int expectedAmount
				new object[] { new DateTime(2020,1,10),   10, 4/*productId*/, 7, 3 },
				new object[] { DateTime.Now.AddDays(2),  10, 4/*productId*/,  7, 10 },
		   };



		[Fact]
		public async Task GetProductAmount_Marketing_ShouldDecreaseAmount()
		{
			//Arrange
			var fixture = GetFixture();
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
					.UseInMemoryDatabase(databaseName: DateTime.Now.Ticks.ToString())
					.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
					.Options;

			using (var context = new ApplicationDbContext(options))
			{
				var inventoryDate = DateTime.UtcNow.AddDays(-2);
				var inventoryAmount = 5;
				var marketingDate = DateTime.UtcNow.AddDays(-1);

				InitManagers(context, fixture);
				await InitData(inventoryDate, inventoryAmount);
			
				var productId = 4;
				var whId = 29;
				//Act
				await _warehouseManager.Create(new WarehouseModel
				{
					WarehouseActionTypeId = (int)WarehouseActionType.Marketing,
					DateAdd = marketingDate,
					WarehouseId = whId,
					WarehouseTypeId = WarehouseType.NotebookReady,
					WarehouseItems = new List<WarehouseItemModel> { new WarehouseItemModel {
					 Amount = 1,
					 ProductId =productId
					}
				}
				});


				var (journal, singleInventory) = _calcManager.GetProductWarehouseState(4, inventoryDate, WarehouseType.NotebookReady);

				var caldWjhDetails = new CalcWhjDetails
				{
					ProductId = productId,
					Journal = journal,
					InventoryDate = inventoryDate,
					Inventory = inventoryAmount,
					SingleInventory = singleInventory?.WarehouseItems.First(whItem => whItem.ProductId == productId).Amount,
					SingleInventoryDate = singleInventory?.DateAdd
				};
				//Assert
				Assert.Equal(4, caldWjhDetails.Current);
			}
		}


		[Fact]
		public async Task AssemblyCalendar_OutcomeGluepads()
		{
			//Arrange
			var fixture = GetFixture();
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
					.UseInMemoryDatabase(databaseName: DateTime.Now.Ticks.ToString())
					.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
					.Options;

			using (var context = new ApplicationDbContext(options))
			{
				InitManagers(context, fixture);
				await _productManager.CreateAsync(new ProductModel { ProductId = Const.CalendarId, ProductSeriesId = 1, ProductKindId = 1, Name = "Календарь" });
				await _productManager.CreateAsync(new ProductModel { ProductId = Const.GluePadProductId, ProductKindId = 3, Name = "Клеевая подушка" });
				await context.OptWarehouseType.AddAsync(new OptWarehouseType { WarehouseTypeId = 8 ,WarehouseObjectType = (int)WhJournalObjectType.GluePad });
				var wh = new WarehouseModel
				{
					Comment = "Test",
					WarehouseTypeId = WarehouseType.GluePads,
					WarehouseItems = new List<WarehouseItemModel>
					{
						new WarehouseItemModel
						{
							Amount = 200,
							ProductId = Const.GluePadProductId,
						},
					},
				};
				await _universalManager.AddInventory(wh);

				await _gluePadManager.Create(new GluePadIncomingModel
				{
					IncomingDate = DateTime.Now.AddDays(1),
					Amount = 50,
				});

				//Act
				await _warehouseManager.Create(new WarehouseModel
				{
					WarehouseActionTypeId = (int)WarehouseActionType.KitAssembly,
					WarehouseId = 2,
					DateAdd = DateTime.Now,
					WarehouseTypeId = WarehouseType.NotebookReady,
					WarehouseItems = new List<WarehouseItemModel>
					{
						new WarehouseItemModel{ Amount = 40, ProductId = Const.CalendarId }
					}
				});

				//Assert
				var whState = await _universalManager.GetWarehouseInfo(WarehouseType.GluePads, WhJournalObjectType.GluePad, new List<long> { Const.CalendarId });
				Assert.Equal(210, whState.Current);
			}			
		}

		private async Task InitData(DateTime inventoryDate, int inventoryAmount)
		{
			await _productManager.CreateAsync(new ProductModel { ProductId = 4, ProductSeriesId = 1, ProductKindId = 1, Name = "Я могу рисовать линии! 2-3 года." });
			int warehouseId = 5;
			var whItems = new List<WarehouseItemModel>();
			whItems.Add(new WarehouseItemModel
			{
				WarehouseId = warehouseId,
				ProductId = 4,
				Amount = inventoryAmount
			});
	 
			await _warehouseManager.Create(new WarehouseModel
			{
				DateAdd = inventoryDate,
				WarehouseActionTypeId = (int)WarehouseActionType.Inventory,
				WarehouseId = 5,
				WarehouseTypeId = WarehouseType.NotebookReady,
				WarehouseItems = whItems
			});
		}

		private async Task InitKitAssemblyData(DateTime inventoryDate, int inventoryAmount)
		{
			await _productManager.CreateAsync(new ProductModel { ProductId = 3, ProductSeriesId = 1, ProductKindId = (int)ProductKind.Notebook, Name = "Я могу! Комплект из 5 пособий. Серия 2-3 года." });
			await _productManager.CreateAsync(new ProductModel { ProductId = 4, ProductSeriesId = 1, ProductKindId = (int)ProductKind.Notebook, Name = "Я могу рисовать линии! 2-3 года." });
			await _productManager.CreateAsync(new ProductModel { ProductId = 5, ProductSeriesId = 1, ProductKindId = (int)ProductKind.Notebook, Name = "Я могу вырезать и клеить! 2-3 года." });
			await _productManager.CreateAsync(new ProductModel { ProductId = 6, ProductSeriesId = 1, ProductKindId = (int)ProductKind.Notebook, Name = "Я могу находить решения! 2-3 года." });
			await _productManager.CreateAsync(new ProductModel { ProductId = 7, ProductSeriesId = 1, ProductKindId = (int)ProductKind.Notebook, Name = "Я могу запоминать! 2-3 года." });
			await _productManager.CreateAsync(new ProductModel { ProductId = 8, ProductSeriesId = 1, ProductKindId = (int)ProductKind.Notebook, Name = "Я могу проходить лабиринты! 2-3 года." });

			await _productManager.AddKitProductAsync(new KitProductModel { MainProductId = 3, ProductId = 4, KitProductId = 100 });
			await _productManager.AddKitProductAsync(new KitProductModel { MainProductId = 3, ProductId = 5, KitProductId = 101 });
			await _productManager.AddKitProductAsync(new KitProductModel { MainProductId = 3, ProductId = 6, KitProductId = 102 });
			await _productManager.AddKitProductAsync(new KitProductModel { MainProductId = 3, ProductId = 7, KitProductId = 103 });
			await _productManager.AddKitProductAsync(new KitProductModel { MainProductId = 3, ProductId = 8, KitProductId = 104 });
			await _priceManager.AddPrice(3, 1400);
			await _priceManager.AddPrice(4, 300);
			await _priceManager.AddPrice(5, 340);
			await _priceManager.AddPrice(6, 300);
			await _priceManager.AddPrice(7, 300);
			await _priceManager.AddPrice(8, 300);

			int warehouseId = 5;
			var whItems = new List<WarehouseItemModel>();
			whItems.Add(new WarehouseItemModel { WarehouseId = warehouseId, ProductId = 3, Amount = inventoryAmount });
			whItems.Add(new WarehouseItemModel { WarehouseId = warehouseId, ProductId = 4, Amount = inventoryAmount });
			whItems.Add(new WarehouseItemModel { WarehouseId = warehouseId, ProductId = 5, Amount = inventoryAmount });
			whItems.Add(new WarehouseItemModel { WarehouseId = warehouseId, ProductId = 6, Amount = inventoryAmount });
			whItems.Add(new WarehouseItemModel { WarehouseId = warehouseId, ProductId = 7, Amount = inventoryAmount });
			whItems.Add(new WarehouseItemModel { WarehouseId = warehouseId, ProductId = 8, Amount = inventoryAmount });
			
			await _warehouseManager.Create(new WarehouseModel
			{
				DateAdd = inventoryDate,
				WarehouseActionTypeId = (int)WarehouseActionType.Inventory,
				WarehouseId = warehouseId,
				WarehouseTypeId =  WarehouseType.NotebookReady,
				WarehouseItems = whItems
			});
		}

		private void InitManagers(ApplicationDbContext context, Fixture fixture)
		{
			_logger = fixture.Create<ILogger<BaseManager>>();
			var configuration = fixture.Create<IConfiguration>();
			var userManager = fixture.Create<UserManager<ApplicationUser>>();
			var profile = new MapperProfile();
			var mapperconfiguration = new MapperConfiguration(cfg => cfg.AddProfile(profile));
			_mapper = new Mapper(mapperconfiguration);
			var productRepository = new FakeProductRepository();
			var priceRepository = new FakePriceRepository(productRepository);
			var warehouseRepository = new FakeWarehouseRepository();
			var whJournalRepository = new WarehouseJournalRepository(context);
			var printOrderRepository = new PrintOrderRepository(context);
			_eventManager = new EventManager(_mapper, context, _logger);
			_productManager = new ProductManager(_mapper, productRepository, priceRepository, context, _eventManager, _logger);

			_priceManager = new PriceManager(_mapper, priceRepository, context, _logger);
			_commonManager = new CommonManager<OptPaper>(_mapper, context, _logger);
			_whjJournalManager = new WarehouseJournalManager(_mapper, _logger, whJournalRepository, printOrderRepository);
			_orderManager = new OrderManager(_mapper, context, productRepository,  priceRepository, null, _logger, userManager, _productManager, _eventManager, _whjJournalManager, configuration);
			 var semiproductRepository = new FakeSemiproductRepository();
			_semiproductManager = new SemiproductManager(_mapper, context, semiproductRepository,  _productManager, _logger);
			_calcManager = new CalcManager(_mapper, warehouseRepository, context, _logger, _productManager, _whjJournalManager, _commonManager, new PrintOrderRepository(context),
				new Data.Repositories.PrintOrderPaperRepository(context));
			
			_semiproductWarehouseManager = new SemiproductWarehouseManager(_mapper, context, _logger, _productManager, _whjJournalManager, _calcManager, _semiproductManager, warehouseRepository); ;
			_warehouseManager = new WarehouseManager(_mapper, warehouseRepository, productRepository, context, _logger, _productManager, _whjJournalManager, _semiproductWarehouseManager, _calcManager, _commonManager);
			_gluePadManager = new GluePadWarehouseManager(_mapper, context, _logger, _whjJournalManager);
			_universalManager = new UniversalWarehouseManager(_mapper, _logger, _whjJournalManager, _productManager, _semiproductManager, null, warehouseRepository);
		}

		private Fixture GetFixture()
		{
			var fixture = new Fixture();
			fixture.Register(() => new Mock<IMapper>().Object);
			fixture.Register(() => new Mock<IConfiguration>().Object);
			fixture.Register(() => new Mock<ILogger<BaseManager>>().Object);
			var userManagerMock = GetMockUserManager();
			fixture.Register(() => userManagerMock.Object);

			return fixture;
		}

		private Mock<UserManager<ApplicationUser>> GetMockUserManager()
		{
			var store = new Mock<IUserStore<ApplicationUser>>();
			var mgr = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
			mgr.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser { ClientType = (int)ClientType.Shop });

			return mgr;
		}
	}
}
