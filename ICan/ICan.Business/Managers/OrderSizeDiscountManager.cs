using AutoMapper;

using ICan.Common.Domain;
using ICan.Common.Models.Opt;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class OrderSizeDiscountManager : BaseManager
	{
		public OrderSizeDiscountManager(IMapper mapper, ApplicationDbContext context, ILogger<BaseManager> logger) : base(mapper, context, logger)
		{
		}

		public async Task<IEnumerable<OrderSizeDiscountClientModel>> Get()
		{
			var orderSizes = await _context.OptOrderSizeDiscount
				   //.Where(t=> t.DateEnd == null)
				   .ToListAsync();
			var groupped = orderSizes.GroupBy(t => t.ClientType);
			var orderSizeData = groupped.Select(item => new OrderSizeDiscountClientModel { ClientType = item.Key, OrderSizes = item.ToList() });
			return orderSizeData;
		}
	}
}
