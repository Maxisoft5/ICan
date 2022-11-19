using AutoMapper;
using ICan.Common.Domain;
using ICan.Common.Models.Exceptions;
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
	public class DiscountManager: BaseManager
	{
		private IDiscountRepository _discountRepository;

		public DiscountManager(
			ApplicationDbContext context,
			IDiscountRepository discountRepository,
			IMapper mapper, 
			ILogger<ShopManager> logger) 
			: base(mapper, context, logger)
		{
			_discountRepository = discountRepository;
		}

		public async Task<IEnumerable<DiscountModel>> GetDiscounts()
		{
			var rawList = await _discountRepository.Get().Where(discount => !discount.IsArchived).ToListAsync();
			var list = _mapper.Map<IEnumerable<DiscountModel>>(rawList);
			return list;
		}

		public async Task<DiscountModel> GetDiscount(int id)
		{
			var raw = await _discountRepository.Get(id);
			if (raw != null && !raw.IsArchived)
				return _mapper.Map<DiscountModel>(raw);
			return null;
		}

		public async Task Create(DiscountModel model)
		{
			model.CreateDate = DateTime.Now;
			var raw = _mapper.Map<OptDiscount>(model);
			await _discountRepository.Add(raw);
		}

		public async Task Delete(int id)
		{
			var raw = await _discountRepository.Get(id);
			if (raw == null)
				throw new UserException($"Не найдена запись с id = {id}");

			await _discountRepository.Archive(raw);
		}
	}
}
