using AutoFixture;
using AutoMapper;
using ICan.Business.Managers;
using ICan.Business.Services;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.Extensions.Logging;
using Moq;

namespace ICan.Test.Helpers
{
	public class HelperBase
	{
		protected readonly ApplicationDbContext _context;

		public HelperBase(ApplicationDbContext context)
		{
			_context = context;
		}

		public CommonManager<TDb> GetCommonManager<TDb>() where TDb : class
		{
			var fixture = GetFixture();

			return new CommonManager<TDb>(fixture.Create<IMapper>(),
				fixture.Create<ApplicationDbContext>(),
				fixture.Create<ILogger<BaseManager>>());
		}
	
		protected Fixture GetFixture()
		{
			var fixture = new Fixture();
			fixture.Register(() => new Mock<IMapper>().Object);
			fixture.Register(() => new Mock<ILogger<ReportParseService>>().Object);
			fixture.Register(() => new Mock<ILogger<BaseManager>>().Object);
			fixture.Register(() => new Mock<IProductRepository>().Object);
			fixture.Register(() => new Mock<IPriceRepository>().Object);
			fixture.Register(() => new Mock<IWarehouseJournalRepository>().Object);
			fixture.Register(() => new Mock<IPrintOrderRepository>().Object);
			fixture.Register(() => _context);
			return fixture;
		}
	}
}