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
	public class GlobalSettingManager : BaseManager
	{
		public GlobalSettingManager(IMapper mapper, ApplicationDbContext context, ILogger<BaseManager> logger) : base(mapper, context, logger)
		{
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
				oldPricesList.ForEach(old => old.DateEnd = oldTime);

				foreach (var price in prices)
				{
					var product = _context.OptProduct.First(t => t.ProductId == price.ProductId);

					price.DateStart = newTime;

					_context.OptProductprice.Add(new OptProductprice
					{
						DateStart = newTime,
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

		public async Task<IEnumerable<GlobalSettingModel>> Get()
		{
			var list = await _context.OptGlobalSetting.ToListAsync();
			return _mapper.Map<IEnumerable<GlobalSettingModel>>(list);
		}

		public async Task<GlobalSettingModel> Get(long id)
		{
			var raw = await _context.OptGlobalSetting
					.FirstOrDefaultAsync(setting => setting.GlobalSettingId == id);
			return _mapper.Map<GlobalSettingModel>(raw);
		}

		public async Task Update(int id, GlobalSettingModel model)
		{
			var raw = _context.OptGlobalSetting.Find(id);
			raw.Content = model.Content;
			await _context.SaveChangesAsync();
		}
	}
}
