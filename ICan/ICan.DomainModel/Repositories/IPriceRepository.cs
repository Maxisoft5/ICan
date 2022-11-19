using ICan.Common.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface IPriceRepository
	{
		Task AddAsync(OptProductprice price);
		IQueryable<OptProductprice> GetPrices(DateTime now);
		Task<OptProductprice> GetPriceForProductAsync(int productId, DateTime dateStart);
		Task<OptProductprice> GetAsync(int productPriceId);
		Task UpdateAsync(OptProductprice oldPrice);
		Task RemoveAsync(int productPriceId);
		Task UpdateRangeAsync(IEnumerable<OptProductprice> list);
		OptProductprice GetPreviousPrice(int productId, DateTime priceDate);
	}
}