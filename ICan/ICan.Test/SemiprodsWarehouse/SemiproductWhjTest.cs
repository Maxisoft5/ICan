using AutoFixture;
using AutoMapper;
using ICan.Business.Managers;
using ICan.Common.Models.Enums;
using ICan.Common.Domain;
using ICan.Data.Context;
using ICan.Test.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using ICan.Common.Models.Opt;
using ICan.Data.Repositories;
using PaperOrderRepository = ICan.Test.Fakes.PaperOrderRepository;
using ICan.Business;

namespace ICan.Test.SemiprodsWarehouse
{
	public class SemiproductWhjTest
	{
		private EventManager _eventManager;
		private AssemblyManager _assemblyManager;
		private ProductManager _productManager;
		private PrintOrderManager _printOrderManager;
		private SemiproductManager _semiproductManager;
		private SemiproductWarehouseManager _semiproductWarehouseManager;
		private WarehouseJournalManager _whjManager;
		private CalcManager _calcManager;
		private PaperOrderManager _paperOrderManager;
		private CommonManager<OptPaper> _paperManager;
		private IMapper _mapper;
		private ILogger<BaseManager> _logger;

		[Fact]
		public async Task CalculateWhJournalSemiproductsV2_ConsideringAssembly_ShouldDecreaseAmount()
		{
			var fixture = GetFixture();
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
					.UseInMemoryDatabase(databaseName: "WhjTest")
					.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
					.Options;

			using var context = new ApplicationDbContext(options);
			await PrepareDataForDecrease(context, fixture);

			var res = await _semiproductWarehouseManager.CalculateWhJournalSemiproducts();
			foreach (var calcResult in res)
			{
				foreach (var semiprodInCalcRes in calcResult.SemiproductList)
				{
					Assert.True(4 == semiprodInCalcRes.CurrentAmount, SemiProductTypeEx.GetName((SemiProductType)semiprodInCalcRes.SemiproductType));
				}
			}
		}

		[Fact]
		public async Task CalculateWhJournalSemiproductsV2_ConsideringIncoming_ShouldIncreaseAmount()
		{
			var fixture = GetFixture();
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
					.UseInMemoryDatabase(databaseName: "WhjTest4")
					.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
					.Options;

			using var context = new ApplicationDbContext(options);
			await PrepareDataForIncrease(context, fixture);

			var res = await _semiproductWarehouseManager.CalculateWhJournalSemiproducts();
			foreach (var calcResult in res)
			{
				foreach (var semiprodInCalcRes in calcResult.SemiproductList)
				{
					Assert.True(20 == semiprodInCalcRes.CurrentAmount, SemiProductTypeEx.GetName((SemiProductType)semiprodInCalcRes.SemiproductType));
				}
			}
		}

		[Fact]
		public async Task CalcSemiproductWhjournalAsync_ConsideringAssembly_ShouldDecreaseAmount()
		{
			var fixture = GetFixture();
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
					.UseInMemoryDatabase(databaseName: "WhjTest2")
					.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
					.Options;

			using var context = new ApplicationDbContext(options);
			await PrepareDataForDecrease(context, fixture);

			var res = await _semiproductWarehouseManager.CalcSemiproductWhjournalAsync(1);
			foreach (var semiprodInCalcRes in res.Value)
			{
				Assert.True(4 == semiprodInCalcRes.Current, semiprodInCalcRes.SemiproductDisplayName);
			}
		}

		[Fact]
		public async Task CalcSemiproductWhjournalAsync_ConsideringIncoming_ShouldIncreaseAmount()
		{
			var fixture = GetFixture();
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
					.UseInMemoryDatabase(databaseName: "WhjTest3")
					.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
					.Options;

			using var context = new ApplicationDbContext(options);
			await PrepareDataForIncrease(context, fixture);

			var res = await _semiproductWarehouseManager.CalcSemiproductWhjournalAsync(1);
			foreach (var semiprodInCalcRes in res.Value)
			{
				Assert.True(20 == semiprodInCalcRes.Current, semiprodInCalcRes.SemiproductDisplayName);
			}
		}

		private async Task PrepareDataForIncrease(ApplicationDbContext context, Fixture fixture)
		{
			InitManagers(context, fixture);
			await InitData(context);

			await _printOrderManager.AddIncoming(new PrintOrderIncomingModel
			{
				IncomingDate = DateTime.Now.AddDays(-1),
				PrintOrderId = 1,
				PrintOrderIncomingId = 1,
				PrintOrderIncomingItems = new List<PrintOrderIncomingItemModel>
				 {
					 new PrintOrderIncomingItemModel{ Amount = 10, PrintOrderIncomingItemId = 1, PrintOrderIncomingId = 1, PrintOrderSemiproductId = 1},
					 new PrintOrderIncomingItemModel{ Amount = 10, PrintOrderIncomingItemId = 2, PrintOrderIncomingId = 1, PrintOrderSemiproductId = 2},
					 new PrintOrderIncomingItemModel{ Amount = 10, PrintOrderIncomingItemId = 3, PrintOrderIncomingId = 1, PrintOrderSemiproductId = 3}
				 }
			});
		}

		private async Task PrepareDataForDecrease(ApplicationDbContext context, Fixture fixture)
		{
			InitManagers(context, fixture);
			await InitData(context);
			await _assemblyManager.CreateAsync(new AssemblyModel
			{
				ProductId = 1,
				Amount = 6,
				AssemblyId = 1,
				Date = DateTime.Now.AddDays(-2),
				WarehouseDateAdd = DateTime.Now.AddDays(-2),
				AssemblySemiproducts = new List<AssemblySemiproductModel> {
					new AssemblySemiproductModel { AssemblyId = 1, AssemblySemiproductId = 1, PrintOrderSemiproductId = 1 },
					new AssemblySemiproductModel { AssemblyId = 1, AssemblySemiproductId = 2, PrintOrderSemiproductId = 2 },
					new AssemblySemiproductModel {  AssemblyId = 1, AssemblySemiproductId = 3, PrintOrderSemiproductId = 3 }
				}
			});

		}

		private async Task InitData(ApplicationDbContext context)
		{
			var product = new ProductModel
			{
				ProductId = 1,
				ProductSeriesId = 1,
				ProductKindId = (int)ProductKind.Notebook,
				Name = "Я могу рисовать линии! 2-3 года.",
				Semiproducts =
			 new List<SemiproductModel>
			{
				new SemiproductModel{  ProductId = 1, SemiproductId = 1, SemiproductTypeId = (int)SemiProductType.Block},
				new SemiproductModel{  ProductId = 1, SemiproductId = 2, SemiproductTypeId = (int)SemiProductType.Covers, },
				new SemiproductModel{  ProductId = 1, SemiproductId = 3, SemiproductTypeId = (int)SemiProductType.Stickers }
			}
			};
			await _productManager.CreateAsync(product);
			await context.OptSemiProductWarehouse.AddAsync(new OptSemiproductWarehouse
			{
				WarehouseActionTypeId = (int)WarehouseActionType.Inventory,
				Date = DateTime.Now.AddDays(-5),
				SemiproductWarehouseId = 1,
				WarehouseActionType = new OptWarehouseActionType { WarehouseActionTypeId = (int)WarehouseActionType.Inventory }
			});
			await context.OptSemiproductWarehouseItem.AddRangeAsync(new List<OptSemiproductWarehouseItem> {
				new OptSemiproductWarehouseItem{ SemiproductWarehouseId = 1, SemiproductId = 1, Amount = 10 },
				new OptSemiproductWarehouseItem{ SemiproductWarehouseId = 1, SemiproductId = 2, Amount = 10 },
				new OptSemiproductWarehouseItem{ SemiproductWarehouseId = 1, SemiproductId = 3, Amount = 10 },
			});
			await context.OptPrintOrder.AddAsync(new OptPrintOrder
			{
				PrintOrderId = 1,
				OrderDate = DateTime.Now.AddDays(-2),
				PrintOrderSemiproducts = new List<OptPrintOrderSemiproduct> {
					new OptPrintOrderSemiproduct{ PrintOrderId = 1, SemiproductId = 1, PrintOrderSemiproductId = 1, SemiProduct =   new OptSemiproduct{   ProductId = 1, SemiproductId = 1, SemiproductTypeId = (int)SemiProductType.Block}, },
					new OptPrintOrderSemiproduct{ PrintOrderId = 1, SemiproductId = 2, PrintOrderSemiproductId = 2, SemiProduct = new OptSemiproduct{  ProductId = 1, SemiproductId = 2, SemiproductTypeId = (int)SemiProductType.Covers} },
					new OptPrintOrderSemiproduct{ PrintOrderId = 1, SemiproductId = 3, PrintOrderSemiproductId = 3, SemiProduct = new OptSemiproduct{  ProductId = 1, SemiproductId = 3, SemiproductTypeId = (int)SemiProductType.Stickers } }
				}
			});

			await context.SaveChangesAsync();
		}

		private void InitManagers(ApplicationDbContext context, Fixture fixture)
		{
			var profile = new MapperProfile();
			var mapperconfiguration = new MapperConfiguration(cfg => cfg.AddProfile(profile));
			_mapper = new Mapper(mapperconfiguration);
			_logger = fixture.Create<ILogger<BaseManager>>();
			var configuration = fixture.Create<IConfiguration>();
			_paperManager = new CommonManager<OptPaper>(_mapper, context, _logger);
			_eventManager = new EventManager(_mapper, context, _logger);
			var productRepository = new FakeProductRepository();
			var priceRepository = new FakePriceRepository(productRepository);
			var warehouseRepository = new FakeWarehouseRepository();
			var semiprductRepository = new FakeSemiproductRepository();
			var paperOrderRepository = new PaperOrderRepository();
			var whJournalRepository = new WarehouseJournalRepository(context);
			var printOrderRepository = new PrintOrderRepository(context);

			_productManager = new ProductManager(_mapper, productRepository, priceRepository, context, _eventManager, _logger);
			_semiproductManager = new SemiproductManager(_mapper, context, semiprductRepository, _productManager, _logger);
			_whjManager = new WarehouseJournalManager(_mapper, _logger, whJournalRepository, printOrderRepository);
			_calcManager = new CalcManager(_mapper, warehouseRepository, context, _logger, _productManager, _whjManager, _paperManager,
				new PrintOrderRepository(context), new PrintOrderPaperRepository(context));
			_semiproductWarehouseManager = new SemiproductWarehouseManager(_mapper, context, _logger, _productManager, _whjManager, _calcManager, _semiproductManager, warehouseRepository);
			_printOrderManager = new PrintOrderManager(_mapper, null, null, context, _logger, _whjManager, configuration, _paperOrderManager);
			_assemblyManager = new AssemblyManager(_mapper, context, _semiproductWarehouseManager, _whjManager, _logger, _printOrderManager, null, productRepository, null);
			_paperOrderManager = new PaperOrderManager(_mapper, paperOrderRepository, context, _logger, _semiproductManager, _whjManager, printOrderRepository);
		}

		private Fixture GetFixture()
		{
			var fixture = new Fixture();
			fixture.Register(() => new Mock<ILogger<BaseManager>>().Object);
			fixture.Register(() => new Mock<IConfiguration>().Object);
			return fixture;
		}
	}
}
