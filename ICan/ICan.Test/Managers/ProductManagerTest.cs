using AutoFixture;
using AutoMapper;
using ICan.Business.Managers;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Test.Fakes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using ICan.Data.Context;
using ICan.Common.Models.Opt;
using ICan.Business;

namespace ICan.Test.Managers
{
	[Collection("Context collection")]
	public class ProductManagerTest
	{
		private Mapper _mapper;
		private EventManager _eventManager;
		private ProductManager _productManager;

		public ILogger<BaseManager> _logger { get; private set; }

		[Fact]
		public async Task GivenNotebooksAndScissors_GetOnlyNotebooks_NoScissorsReturned()
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
				await InitProducts(context);

				//Act
				var products = await _productManager.GetAsync(false, onlyNotebooks: true);
				var scissors = products.SelectMany(group => group.Value)
					.Any(product => product.ProductKindId == (int)ProductKind.SmthElse);

				//Assert
				Assert.False(scissors);
			}
		}

		[Fact]
		public void GivenNotebookPricesAndOrder_AddNewPrices_OldPricesForOrderReturned()
		{
			//Arrange
			Assert.True(true);
		}


		private async Task InitProducts(ApplicationDbContext context)
		{
			await _productManager.CreateAsync(new ProductModel { ProductId = 3, ProductSeriesId = 1, ProductKindId = (int)ProductKind.Notebook, Name = "Я могу! Комплект из 5 пособий. Серия 2-3 года." });
			await _productManager.CreateAsync(new ProductModel { ProductId = 4, ProductSeriesId = 1, ProductKindId = (int)ProductKind.Notebook, Name = "Я могу рисовать линии! 2-3 года." });

			await _productManager.CreateAsync(new ProductModel { ProductId = 5, ProductKindId = (int)ProductKind.SmthElse, Name = "Ножницы" });
		}

		private void InitManagers(ApplicationDbContext context, Fixture fixture)
		{
			_logger = fixture.Create<ILogger<BaseManager>>();
			var globalConfiguration = fixture.Create<IConfiguration>();
			var userManager = fixture.Create<UserManager<ApplicationUser>>();
			var profile = new MapperProfile();
			var configuration = new MapperConfiguration(cfg => cfg.AddProfile(profile));
			var productRepository = new FakeProductRepository();
			var priceRepository = new FakePriceRepository(productRepository);
			_mapper = new Mapper(configuration);
			_eventManager = new EventManager(_mapper, context, _logger);
			_productManager = new ProductManager(_mapper, productRepository, priceRepository, context, _eventManager, _logger);
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
