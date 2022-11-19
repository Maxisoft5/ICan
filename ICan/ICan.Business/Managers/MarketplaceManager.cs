using AutoMapper;
using ICan.Common.Models.Exceptions;
using ICan.Common.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ICan.Data.Context;
using ICan.Common.Models.Opt;

namespace ICan.Business.Managers
{
	public class MarketplaceManager: BaseManager
	{
		public MarketplaceManager(IMapper mapper, ApplicationDbContext context, ILogger<BaseManager> logger, IConfiguration configuration)
			: base(mapper, context, logger, configuration)
		{
		}

		public IEnumerable<MarketplaceModel> Get()
		{
			var marketplaces = _context.OptMarketplace;
			var marketplaceModels = _mapper.Map<List<MarketplaceModel>>(marketplaces);
			return marketplaceModels;
		}

		public IEnumerable<MarketplaceModel> Get(string baseDir)
		{
			var marketplaceModels = Get().ToList();
			marketplaceModels.ForEach(x => { SetImagePath(x, baseDir); });
			return marketplaceModels;
		}

		public async Task Create(IFormFile image, MarketplaceModel model)
		{
			var marketPlace = _mapper.Map<OptMarketplace>(model);
			if(image != null)
			{
				var imageName = await SaveImageAsync(image);
				marketPlace.ImageName = imageName;
			}			
			await _context.AddAsync(marketPlace);
			await _context.SaveChangesAsync();
		}

		public async Task<MarketplaceModel> GetById(int id, string baseDir)
		{
			var marketPlace = await _context.OptMarketplace.FirstOrDefaultAsync(x => x.MarketplaceId == id);
			var model = _mapper.Map<MarketplaceModel>(marketPlace);			
			SetImagePath(model, baseDir);
			return model;
		}

		public async Task Update(MarketplaceModel model)
		{
			var marketPlace = _mapper.Map<OptMarketplace>(model);
			_context.Update(marketPlace);
			await _context.SaveChangesAsync();
		}

		public async Task Delete(int id)
		{
			var marketPlace = await _context.OptMarketplace.FirstOrDefaultAsync(x => x.MarketplaceId == id);
			
			if (marketPlace == null)
				throw new UserException($"Невозможно удалить маркетплейс с указанным id = {id}");

			_context.Remove(marketPlace);
			await _context.SaveChangesAsync();
		}

		private void SetImagePath(MarketplaceModel model, string baseDir)
		{
			if (!string.IsNullOrWhiteSpace(model.ImageName))
				model.ImagePath = Path.Combine("/", baseDir, "sitedata", model.ImageName);
		}

		private async Task<string> SaveImageAsync(IFormFile image)
		{
			var fileFormat = image.FileName.Split(".").Last();
			if (!fileFormat.Equals("png", StringComparison.InvariantCultureIgnoreCase) && !fileFormat.Equals("svg", StringComparison.InvariantCultureIgnoreCase))
				throw new UserException("Допустимый формат изображения: PNG или SVG");

			if(fileFormat.Equals("png", StringComparison.InvariantCultureIgnoreCase))
			{
				using (var bitmap = new Bitmap(image.OpenReadStream()))
				{
					if (bitmap.Width < 85 && bitmap.Height < 40)
						throw new UserException("Изображение должно быть не меньше 85х40");
				}
			}			

			var fileName = Guid.NewGuid().ToString() + "." + fileFormat;
			var filesPath = Path.Combine(_configuration["Settings:FilesFolder"], "sitedata", fileName);
			using (var fileStream = new FileStream(filesPath, FileMode.Create))
			{
				await image.CopyToAsync(fileStream);
			}
			return fileName;
		}
	}
}
