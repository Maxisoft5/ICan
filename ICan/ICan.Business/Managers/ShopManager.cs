using AutoMapper;
using ICan.Common.Models.Exceptions;
using ICan.Common.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;
using System.Text.RegularExpressions;
using ICan.Common.Models.Opt;
using ICan.Data.Context;
using Microsoft.AspNetCore.Mvc.Rendering;
using ICan.Common.Repositories;

namespace ICan.Business.Managers
{
	public class ShopManager : BaseManager
	{
		private readonly string _filesFolder;
		private readonly IShopRepository _shopRepository;

		public ShopManager(ApplicationDbContext context, IMapper mapper,
			IShopRepository shopRepository,
			ILogger<ShopManager> logger, IConfiguration configuration)
			: base(mapper, context, logger)
		{
			_filesFolder = configuration["Settings:FilesFolder"];
			_shopRepository = shopRepository;
		}

		public async Task<IEnumerable<ShopModel>> GetShops()
		{
			var list = await _context.OptShop.AsNoTracking().Include(shop => shop.ShopNames).ToListAsync();
			var shops = _mapper.Map<IEnumerable<ShopModel>>(list);
			return shops;
		}
		
		public async Task<IDictionary<string, string>> Dict()
		{
			var list = await _context.OptShop.AsNoTracking().ToListAsync();
			var shops = list.ToDictionary(shop => shop.Name, shop => shop.Name);
			return shops;
		}

		public async Task Create(ShopModel model, IFormFile scanFile)
		{
			var optShop = _mapper.Map<OptShop>(model);
			if (scanFile != null)
				await SaveFileAsync(optShop, scanFile);
			_context.Add(optShop);
			var shopName = new OptShopName { Shop = optShop, Name = model.Name, Inn = model.INN };
			_context.Add(shopName);
			await _context.SaveChangesAsync();
		}

		public async Task<ShopModel> GetShop(int id)
		{
			var optShop = await _context.OptShop.Include(shop => shop.ShopNames).FirstOrDefaultAsync(m => m.ShopId == id);
			if (optShop == null)
			{
				throw new UserException($"Магазин с id = {id} не найден");
			}
			var shopModel = _mapper.Map<ShopModel>(optShop);
			return shopModel;
		}

		public async Task Edit(OptShop optShop, IFormFile scanFile)
		{
			if (scanFile != null)
				await SaveFileAsync(optShop, scanFile);
			_context.Update(optShop);
			await _context.SaveChangesAsync();
		}

		public async Task Delete(int id)
		{
			var optShop = await _context.OptShop.FirstOrDefaultAsync(m => m.ShopId == id);
			_context.OptShop.Remove(optShop);
			await _context.SaveChangesAsync();
		}

		public async Task AddShopName(OptShopName raw)
		{
			_context.Add(raw);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteShopName(int id)
		{
			var shopName = await _context.OptShopName.FirstOrDefaultAsync(shopN => shopN.ShopNameId == id);
			_context.Remove(shopName);
			await _context.SaveChangesAsync();
		}

		public bool OptShopExists(int id)
		{
			return _context.OptShop.Any(e => e.ShopId == id);
		}

		public string GetPathFile(int shopId)
		{
			var shop = _context.OptShop.FirstOrDefault(x => x.ShopId == shopId);
			if (string.IsNullOrWhiteSpace(shop.ScanFileName))
				return null;
			return BuildPath(shop.ScanFileName);
		}

		private async Task SaveFileAsync(OptShop optShop, IFormFile scanFile)
		{
			var editedFileName = Regex.Replace(scanFile.FileName, "[^а-яА-Яa-zA-Z0-9._]", "");
			var pathFile = BuildPath(editedFileName);
			DeleteFileIfExists(pathFile);
			using (var fileStream = new FileStream(pathFile, FileMode.Create))
			{
				await scanFile.CopyToAsync(fileStream);
			}
			optShop.ScanFileName = editedFileName;
			var fileProvider = new FileExtensionContentTypeProvider();
			if (fileProvider.TryGetContentType(pathFile, out var mimeType))
				optShop.MimeType = mimeType;
		}

		public async Task DeleteContractFile(int shopId)
		{
			var shop = await _context.OptShop.FirstOrDefaultAsync(x => x.ShopId == shopId);
			if (!string.IsNullOrWhiteSpace(shop.ScanFileName))
				DeleteFileIfExists(shop.ScanFileName);
			shop.ScanFileName = null;
			shop.MimeType = null;
			await _context.SaveChangesAsync();
		}

		private void DeleteFileIfExists(string filePath)
		{
			if (File.Exists(filePath))
				File.Delete(filePath);
		}

		private string BuildPath(string fileName)
		{
			return Path.Combine(_filesFolder, fileName);
		}

		public async Task UnbindShop(int shopId, string userId)
		{
			var shop = await _context.OptApplicationUserShopRelation.FirstAsync(x => x.ShopId == shopId && x.UserId == userId);
			_context.Remove(shop);
			await _context.SaveChangesAsync();
		}

		public IEnumerable<SelectListItem> GetClientShops(string clientId)
		{
			var shops = _shopRepository.GetClientShops(clientId).ToList();
			return shops.Select(shop => new SelectListItem { Text = shop.Name, Value = shop.ShopId.ToString() });
		}
	}
}
