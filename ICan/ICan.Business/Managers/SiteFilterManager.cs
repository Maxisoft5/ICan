using AutoMapper;
using ICan.Common.Domain;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class SiteFilterManager : BaseManager
	{
		private ISiteFilterRepository _siteFilterRepository;

		public SiteFilterManager(IMapper mapper, ISiteFilterRepository siteFilterReposutory, ILogger<BaseManager> logger)
			: base(mapper, logger)
		{
			_siteFilterRepository = siteFilterReposutory;
		}

		public async Task<IEnumerable<SiteFilterModel>> GetAllAsync()
		{
			var raw = await _siteFilterRepository.GetAll().ToListAsync();
			var list = _mapper.Map<IEnumerable<SiteFilterModel>>(raw);
			return list;
		}

		public async Task<SiteFilterModel> GetByIdAsync(int id)
		{
			var raw = await _siteFilterRepository.GetAsync(id);
			var model = _mapper.Map<SiteFilterModel>(raw);
			model.Products = model.Products.OrderBy(prod => prod.Order).ToList();
			return model;
		}

		public async Task AddAsync(SiteFilterModel model)
		{
			var raw = _mapper.Map<OptSiteFilter>(model);
			await _siteFilterRepository.AddAsync(raw);
		}

		public async Task DeleteProductAsync(int siteFilterProductId)
		{
			await _siteFilterRepository.DeleteFilterProductAsync(siteFilterProductId);
		}

		public async Task<IEnumerable<SelectListItem>> GetAvailableProductsAsync(int id)
		{
			var availableProducts = await _siteFilterRepository.GetAvailableProductsAsync(id);
			return _mapper.Map<IEnumerable<ProductModel>>(availableProducts)
				.Select(product => new SelectListItem { Text = product.DisplayName, Value = product.ProductId.ToString() });
		}

		public async Task DeleteAsync(int id)
		{
			await _siteFilterRepository.DeleteAsync(id);
		}

		public async Task AddFilterProductAsync(SiteFilterProductModel model)
		{
			var raw = _mapper.Map<OptSiteFilterProduct>(model);
			await _siteFilterRepository.AddFilterProductAsync(raw);
		}

		public async Task EditFilterProductAsync(SiteFilterProductModel model)
		{
			var raw = _mapper.Map<OptSiteFilterProduct>(model);
			await _siteFilterRepository.UpdateFilterProductAsync(raw);
		}

		public async Task<SiteFilterProductModel> GetFilterProduct(int siteFilterProductId)
		{
			var raw = await _siteFilterRepository.GetFilterProductAsync(siteFilterProductId);
			var model = _mapper.Map<SiteFilterProductModel>(raw);
			return model;
		}
	}
}
