using AutoFixture;
using AutoMapper;
using ICan.Business;
using ICan.Business.Managers;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Opt;
using ICan.Data.Context;
using ICan.Data.Repositories;
using ICan.Test.Fakes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ICan.Test.Order
{
	public class OrderTest
	{
		private ILogger<BaseManager> _logger { get; set; }
		private Mapper _mapper { get; set; }
		private EventManager _eventManager { get; set; }
		private ProductManager _productManager { get; set; }

		private PriceManager _priceManager;

		private WarehouseJournalManager _whjJournalManager { get; set; }

		private OrderManager _orderManager { get; set; }


		[Fact]
		public async Task GivenClientAndProductIds_CreateOrder_OrderIsCreated()
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
				await InitKitAssemblytData(context);
			
				var clientId = Guid.NewGuid().ToString();
				var orderModel = new ClientOrderModel
				{
					OrderItems = new List<ShortOrderProductModel> { new ShortOrderProductModel { Amount = 2, Id = 3 } } .ToArray(),
					Address = "Адрес клиента"
				};

				//Act
				await _orderManager.CreateOrderAsync(clientId, orderModel);

				var orderCount = await context.OptOrder.CountAsync();
				//Assert
				Assert.Equal(1, orderCount);
			}
		}

		private   async Task InitKitAssemblytData(ApplicationDbContext context)
		{
			await _productManager.CreateAsync(new ProductModel { ProductId = 3, ProductSeriesId = 1, ProductKindId = 1, Name = "Я могу! Комплект из 5 пособий. Серия 2-3 года." });
			await _productManager.CreateAsync(new ProductModel  { ProductId = 4, ProductSeriesId = 1, ProductKindId = 1, Name = "Я могу рисовать линии! 2-3 года." });
			await _productManager.CreateAsync(new ProductModel  { ProductId = 5, ProductSeriesId = 1, ProductKindId = 1, Name = "Я могу вырезать и клеить! 2-3 года." });
			await _productManager.CreateAsync(new ProductModel  { ProductId = 6, ProductSeriesId = 1, ProductKindId = 1, Name = "Я могу находить решения! 2-3 года." });
			await _productManager.CreateAsync(new ProductModel  { ProductId = 7, ProductSeriesId = 1, ProductKindId = 1, Name = "Я могу запоминать! 2-3 года." });
			await _productManager.CreateAsync(new ProductModel { ProductId = 8, ProductSeriesId = 1, ProductKindId = 1, Name = "Я могу проходить лабиринты! 2-3 года." });

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
		}

		private void InitManagers(ApplicationDbContext context, Fixture fixture)
		{
			_logger = fixture.Create<ILogger<BaseManager>>();
			var globalConfiguration = fixture.Create<IConfiguration>();
			var userManager = fixture.Create<UserManager<ApplicationUser>>();
			var profile = new MapperProfile();
			var configuration = new MapperConfiguration(cfg => cfg.AddProfile(profile));
			_mapper = new Mapper(configuration);
			var productRepository = new FakeProductRepository();
			var whJournalRepository = new WarehouseJournalRepository(context);
			var printOrderRepository = new PrintOrderRepository(context);
			var priceRepository = new FakePriceRepository(productRepository);
			_eventManager = new EventManager(_mapper, context, _logger);
			_productManager = new ProductManager(_mapper, productRepository, priceRepository,context, _eventManager, _logger);
			_priceManager = new PriceManager(_mapper, priceRepository,context, _logger);

			_whjJournalManager = new WarehouseJournalManager(_mapper, _logger, whJournalRepository, printOrderRepository);
			_orderManager = new OrderManager(_mapper, context, productRepository, priceRepository, null, _logger, userManager, _productManager, _eventManager, _whjJournalManager, globalConfiguration);
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
