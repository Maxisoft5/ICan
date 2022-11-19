using AutoMapper;
using ICan.Common.Domain;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class PriceManager : BaseManager
	{
		private readonly IPriceRepository _priceRepository;

		public PriceManager(IMapper mapper, IPriceRepository priceRepository, ApplicationDbContext context, ILogger<BaseManager> logger) : base(mapper, context, logger)
		{
			_priceRepository = priceRepository;
		}

		public void UpdatePrices(IEnumerable<ProductpriceModel> prices)
		{
			try
			{
				var newTime = DateTime.Now;
				var oldTime = newTime.AddSeconds(-1);

				var oldPrices = _context
						.OptProductprice
						.Include(t => t.Product)
						.Where(t => (!t.DateEnd.HasValue || t.DateEnd > oldTime));

				var oldPricesList = oldPrices.ToList();

				foreach (var price in prices)
				{
					var product = _context.OptProduct.First(t => t.ProductId == price.ProductId);

					price.DateStart = newTime;

					_context.OptProductprice.Add(new OptProductprice
					{
						DateStart = newTime,
						//	DateEnd = price.DateEnd,
						Price = price.Price,
						ProductId = price.ProductId,
					});

				}
				_context.SaveChanges();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обновлении цен");
			}
		}

		public List<ProductpriceModel> GetPrices(int? productId)
		{
			var now = DateTime.Now;
			return _context
				.OptProductprice
				.Include(t => t.Product)
				.Where(q => q.ProductId == productId)
				.Where(q => (q.DateEnd.HasValue && q.DateStart.HasValue &&
					now >= q.DateStart.Value &&
					now < q.DateEnd.Value)
					|| (!q.DateEnd.HasValue && q.DateStart.HasValue && now >= q.DateStart)
					|| (!q.DateEnd.HasValue && !q.DateStart.HasValue))
				.Select(t => new ProductpriceModel(t)).ToList();
		}

		public async Task<ProductpriceModel> GetAsync(int productPriceId)
		{
			OptProductprice raw = await _priceRepository.GetAsync(productPriceId);
			var model = _mapper.Map<ProductpriceModel>(raw);
			return model;
		}


		public async Task RemoveAsync(int productPriceId)
		{
			await _priceRepository.RemoveAsync(productPriceId);
		}
 
		public async Task AddPrice(int productId, double price)
		{
			var date = DateTime.Now.AddSeconds(-1);
			var oldPrice = await _priceRepository.GetPriceForProductAsync(productId, date);
			
			if (oldPrice != null) { 
				oldPrice.DateEnd = date;
				await _priceRepository.UpdateAsync(oldPrice);
			}
			var newPrice = new OptProductprice
			{
				Price = price,
				ProductId = productId,
				DateStart = date
			};
			await _priceRepository.AddAsync(newPrice);
		}
	}
}
