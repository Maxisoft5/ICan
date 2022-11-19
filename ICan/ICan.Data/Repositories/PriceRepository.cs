using ICan.Common.Domain;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Data.Repositories
{
	public class PriceRepository : BaseRepository, IPriceRepository
	{
		public PriceRepository(ApplicationDbContext context) : base(context)
		{
		}

		public async Task<OptProductprice> GetPriceForProductAsync(int productId, DateTime dateStart)
		{
			return await _context.OptProductprice
				.FirstOrDefaultAsync(t => t.ProductId == productId &&
				(t.DateEnd == null && t.DateStart <= dateStart));
		}

		public async Task AddAsync(OptProductprice price)
		{
			await _context.AddAsync(price);
			await _context.SaveChangesAsync();
		}

		public IQueryable<OptProductprice> GetPrices(DateTime now)
		{
			var prices = _context.OptProductprice
				.Where(price => ((price.DateEnd.HasValue && price.DateStart.HasValue &&
				now >= price.DateStart.Value &&
				now < price.DateEnd.Value)
				|| (!price.DateEnd.HasValue && price.DateStart.HasValue && now >= price.DateStart)
				|| (!price.DateEnd.HasValue && !price.DateStart.HasValue)))
				.OrderBy(q => q.ProductId);

			return prices;
		}
		
		public OptProductprice GetPreviousPrice(int productId, DateTime priceDate)
		{
			var price = _context.OptProductprice
				.Where(price => price.ProductId == productId && price.DateEnd.HasValue && price.DateEnd <= priceDate)
				.OrderByDescending(price => price.DateEnd)
				.FirstOrDefault();

			return price;
		}

		public async Task UpdateAsync(OptProductprice oldPrice)
		{
			_context.Update(oldPrice);
			await _context.SaveChangesAsync();
		}

		public async Task<OptProductprice> GetAsync(int productPriceId)
		{
			return await _context
						   .OptProductprice
						   .FirstOrDefaultAsync(q => q.ProductPriceId == productPriceId);
		}

		public async Task RemoveAsync(int productPriceId)
		{
			var raw = await GetAsync(productPriceId);
			if (raw == null)
				return;

			_context.OptProductprice.Remove(raw);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateRangeAsync(IEnumerable<OptProductprice> prices)
		{
			var now = DateTime.Now;
			var oldPrices = GetPrices(now);
			foreach (var price in prices)
			{
				var productOldPrices = oldPrices.Where(old => old.ProductId == price.ProductId);
				if (productOldPrices != null && productOldPrices.Any())
				{
					await productOldPrices.ForEachAsync(
						   old => old.DateEnd = now);
				}
				price.DateStart = now;
				await _context.AddAsync(price);
			}
			await _context.SaveChangesAsync();
		}
	}
}
