using ICan.Business.Managers;
using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Common.Utils;
using ICan.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,ContentMan")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class MaterialController : BaseController
	{
		private readonly MaterialManager _materialManager;
		private readonly ImageManager _imageManager;
		private readonly int _currentSiteId;
		private readonly string _bucketUrl;
		private readonly IMemoryCache _cache;
		private static readonly string CacheKey = "Locale";

		public MaterialController(
			  MaterialManager materialManager,
			ImageManager imageManager,
			ILogger<BaseController> logger,
			IMemoryCache memoryCache)
			: base(logger)
		{
			_materialManager = materialManager;
			_imageManager = imageManager;
			_cache = memoryCache;
		}

		[AllowAnonymous]

		[Route("/SiteHome/Material")]
		[Route("/SiteHome/Material/Index")]
		public IActionResult Index()
		{
			var list = _materialManager.Get();
			return View(list);
		}


		public IActionResult List()
		{
			var list = _materialManager.Get(false);
			return View(list);
		}

		public IActionResult Create()
		{
			var model = new MaterialModel
			{
				Date = DateTime.Now
			};
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Create(MaterialModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					await _materialManager.AddAsync(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(List));
				}
				catch (Exception ex)
				{
					_logger.LogError(ex.Message);
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
				}
			}
			ViewBag.Action = ActionType.Creation;
			return View(model);
		}



		public async Task<IActionResult> Edit(int id)
		{
			var model = await _materialManager.Get(id);
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(MaterialModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					await _materialManager.UpdateAsync(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(List));
				}
				catch (Exception ex)
				{
					_logger.LogError(ex.Message);
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
				}
			}

			return View(model);
		}

		public IActionResult AddImage(int id, ImageObjectType objectTypeId)
		{
			bool imageExists = _imageManager.ImageByTypeExists(id, ProductImageType.Material);
			var model = new ImageModel
			{
				ObjectId = id,
				ObjectTypeId = objectTypeId,
				ImageType = ProductImageType.MaterialPreview,
			};

			var list = new List<SelectListItem>();
			if (!imageExists)
			{
				list.Add(
				new SelectListItem
				{
					Text = ProductImageType.Material.GetDisplayName(),
					Value = ((int)ProductImageType.Material).ToString(),
				});
			};

			list.Add(new SelectListItem
			{
				Text = ProductImageType.MaterialPreview.GetDisplayName(),
				Value = ((int)ProductImageType.MaterialPreview).ToString(),
			});

			ViewData["ImageTypes"] = list;
			return PartialView("~/Views/Shared/_AddImage.cshtml", model);
		}

		[HttpPost]
		public async Task<IActionResult> AddImage(ImageModel model, IFormFile image)
		{
			try
			{
				bool catalogImageExists = _imageManager.ImageByTypeExists(model.ObjectId, ProductImageType.Material);
				if (catalogImageExists && model.ImageType == ProductImageType.Material)
				{
					TempData["ErrorMessage"] = "Невозможно добавить для тетради вторую иллюстрацию типа \"Материал\"";
				}
				else if (ModelState.IsValid)
				{
					await _imageManager.AddImageAsync(model, image);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Site] Ошибка при сохранении иллюстрации");
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
			}

			return RedirectToAction(nameof(Edit), "Material", new { id = model.ObjectId }, "image-info");
		}

		[HttpPost]
		public async Task<IActionResult> DeleteImage(int imageId, int objectId)
		{
			try
			{
				await _imageManager.DeleteImage(imageId);

				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				return RedirectToAction(nameof(Edit), new { id = objectId });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Site] Ошибка при сохранении иллюстрации");
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
			}

			return BadRequest();
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _materialManager.DeleteAsync(id);

				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Site] Ошибка при удалении материала");
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
			}

			return RedirectToAction(nameof(List));
		}


		[AllowAnonymous]
		[Route("/materal/download/{fileName}")]
		public async Task<IActionResult> Download(string fileName)
		{
			try
			{
				_logger.LogWarning("[Material][Download]");
				var model = await _materialManager.GetByFileName(fileName);
				var bytes = await _materialManager.GetMaterialToDownload(model);

				return File(bytes, model.DownloadFile.ContentType);
			}
			catch (Exception ex)
			{
				_logger.LogWarning($"[SiteData][SiteHome] error {ex.ToString()}");
				return NotFound();
			}
		}

		//[AllowAnonymous]
		//[Route("/AboutUs")]
		//[Route("/SiteHome/AboutUs")]
		//public IActionResult AboutUs()
		//{
		//	SetLocale();
		//	return View();
		//}

		//[AllowAnonymous]
		//[Route("/error")]
		//[Route("/SiteHome/error")]
		//public IActionResult ErrorPage()
		//{
		//	_logger.LogError($"error path{Request.Path}");
		//	return View(GetView("ErrorPage.cshtml"));
		//}

		//public IActionResult UploadMarketPlaceFile() => View("_AddMarketPlaceFile");

		//public async Task<IActionResult> ProductList()
		//{
		//	var products = await _siteManager.GetSiteProductsAsync();
		//	return View(products);
		//}

		//[Route("/Contacts")]
		//[Route("/SiteHome/Contacts")]
		//[AllowAnonymous]
		//public IActionResult Contacts()
		//{
		//	SetLocale();
		//	return View();
		//}

		//[Route("/Notebook/{id}")]
		//[Route("/SiteHome/Notebook/{id}")]
		//[AllowAnonymous]
		//public async Task<IActionResult> Notebook(int id)
		//{
		//	var model = await _siteManager.GetAsync(id);
		//	SetLocale();
		//	return View(GetView("Notebook.cshtml"), model);
		//}

		//[HttpGet("[controller]/Edit")]
		//public async Task<IActionResult> Edit(int id)
		//{
		//	var model = await _siteManager.GetAsync(id);
		//	return View("Edit", model);
		//}

		//[HttpPost("[controller]/Edit")]
		//public async Task<IActionResult> Edit(SiteProductModel model, IFormFile videoFileName)
		//{
		//	if (ModelState.IsValid)
		//	{
		//		try
		//		{
		//			await _siteManager.UpdateAsync(model, videoFileName);
		//			TempData["StatusMessage"] = Const.SuccessMessages.Saved;
		//		}
		//		catch (Exception ex)
		//		{
		//			_logger.LogError(ex, "[Site] Ошибка при сохранении тетради для сайта");
		//			TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
		//		}
		//	}
		//	else
		//	{
		//		TempData["ErrorMessage"] = $"Невозможно сохранить информацю {GetErrors()}";
		//	}
		//	return RedirectToAction(nameof(ProductList));
		//}

		//[HttpPost]
		//public async Task<IActionResult> UploadMarketPlaceFile(IFormFile marketPlaceFile)
		//{
		//	try
		//	{
		//		await _siteManager.ParseFileAsync(marketPlaceFile);
		//		TempData["StatusMessage"] = Const.SuccessMessages.Saved;
		//	}
		//	catch (UserException ex)
		//	{
		//		TempData["ErrorMessage"] = ex.Message;
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError(ex, "[Site] Ошибка при загрузке файла");
		//		TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
		//	}
		//	return RedirectToAction(nameof(ProductList));
		//}

		//public IActionResult AddImage(int productId)
		//{
		//	bool catalogImageExists = _siteManager.CatalogImageExists(productId);
		//	var model = new ProductImageModel
		//	{
		//		ProductId = productId,
		//		ImageType = ProductImageType.Catalog,
		//	};

		//	var list = new List<SelectListItem>();
		//	if (!catalogImageExists)
		//	{
		//		list.Add(
		//		new SelectListItem
		//		{
		//			Text = ProductImageType.Catalog.GetDisplayName(),
		//			Value = ((int)ProductImageType.Catalog).ToString(),
		//		});
		//	}
		//	list.Add(new SelectListItem
		//	{
		//		Text = ProductImageType.SmallGallery.GetDisplayName(),
		//		Value = ((int)ProductImageType.SmallGallery).ToString(),
		//	});
		//	list.Add(new SelectListItem
		//	{
		//		Text = ProductImageType.BigGallery.GetDisplayName(),
		//		Value = ((int)ProductImageType.BigGallery).ToString()
		//	});
		//	list.Add(new SelectListItem
		//	{
		//		Text = ProductImageType.TextImage.GetDisplayName(),
		//		Value = ((int)ProductImageType.TextImage).ToString(),
		//	});
		//	list.Add(new SelectListItem
		//	{
		//		Text = ProductImageType.RichContent.GetDisplayName(),
		//		Value = ((int)ProductImageType.RichContent).ToString(),
		//	});
		//	list.Add(new SelectListItem
		//	{
		//		Text = ProductImageType.MobileCatalog.GetDisplayName(),
		//		Value = ((int)ProductImageType.MobileCatalog).ToString(),
		//	});
		//	ViewData["ImageTypes"] = list;
		//	return PartialView(model);
		//}

		//[HttpPost]
		//public async Task<IActionResult> AddImage(ProductImageModel model, IFormFile image)
		//{
		//	try
		//	{
		//		bool catalogImageExists = _siteManager.CatalogImageExists(model.ProductId);
		//		if (catalogImageExists && model.ImageType == ProductImageType.Catalog)
		//		{
		//			TempData["ErrorMessage"] = "Невозможно добавить для тетради вторую иллюстрацию типа \"Каталог\"";
		//		}
		//		else if (ModelState.IsValid)
		//		{
		//			await _siteManager.AddImageAsync(model, image);
		//			TempData["StatusMessage"] = Const.SuccessMessages.Saved;
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError(ex, "[Site] Ошибка при сохранении иллюстрации");
		//		TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
		//	}

		//	return RedirectToAction(nameof(Edit), "SiteHome", new { id = model.ProductId }, "image-info");
		//}

		//[HttpPost]
		//public async Task<IActionResult> DeleteImage(int imageId, int productId)
		//{
		//	try
		//	{

		//		await _siteManager.DeleteImage(imageId);

		//		TempData["StatusMessage"] = Const.SuccessMessages.Saved;
		//		return RedirectToAction(nameof(Edit), new { id = productId });
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError(ex, "[Site] Ошибка при сохранении иллюстрации");
		//		TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
		//	}

		//	return BadRequest();
		//}

		//[HttpPost]
		//public async Task<IActionResult> DeleteVideo(int productId)
		//{
		//	try
		//	{
		//		await _siteManager.DeleteVideo(productId);

		//		TempData["StatusMessage"] = Const.SuccessMessages.Saved;
		//		return Ok();
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError(ex, "[Site] Ошибка при сохранении видео");
		//		TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
		//	}

		//	return BadRequest();
		//}

		//public async Task<IActionResult> AddTag(int productId)
		//{
		//	var model = new ProductTagModel
		//	{
		//		ProductId = productId,
		//	};
		//	var tags = await _tagManager.GetAsync();
		//	var existingTags = _siteManager.GetProductTagIds(productId);
		//	var availableTags = tags.Where(tag => !existingTags.Contains(tag.TagId)).Select(tag => new SelectListItem
		//	{
		//		Text = tag.TagName,
		//		Value = tag.TagId.ToString(),
		//	});
		//	ViewData["TagId"] = availableTags;
		//	ViewData["CanAddMore"] = availableTags.Any();
		//	return PartialView(model);
		//}

		//[HttpPost]
		//public async Task<IActionResult> AddTag(ProductTagModel model)
		//{
		//	try
		//	{
		//		if (ModelState.IsValid)
		//		{
		//			await _siteManager.AddTagAsync(model);
		//			TempData["StatusMessage"] = Const.SuccessMessages.Saved;
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError(ex, "[Site] Ошибка при сохранении иллюстрации");
		//		TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
		//	}

		//	return RedirectToAction(nameof(Edit), "SiteHome", new { id = model.ProductId });
		//}

		//[HttpPost]
		//public async Task<IActionResult> DeleteTag(int productTagId, int productId)
		//{
		//	try
		//	{
		//		if (ModelState.IsValid)
		//		{
		//			await _siteManager.DeleteTag(productTagId);
		//		}

		//		TempData["StatusMessage"] = Const.SuccessMessages.Saved;
		//		return RedirectToAction(nameof(Edit), new { id = productId });
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError(ex, "[Site] Ошибка при сохранении иллюстрации");
		//		TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
		//	}
		//	return BadRequest();
		//}

		//public IActionResult AddMarketplaceProduct(int productId)
		//{
		//	var model = new MarketplaceProductModel
		//	{
		//		ProductId = productId,
		//	};

		//	ViewBag.Action = ActionType.Creation;
		//	SetViewDataMarketPlaces(productId);
		//	return PartialView("_EditMarketplaceProduct", model);
		//}

		//[HttpPost]
		//public async Task<IActionResult> AddMarketplaceProduct(MarketplaceProductModel model)
		//{
		//	if (ModelState.IsValid)
		//	{
		//		await _siteManager.AddMarketPlaceProductAsync(model);
		//		TempData["StatusMessage"] = Const.SuccessMessages.Saved;
		//	}
		//	else
		//	{
		//		TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
		//	}

		//	return RedirectToAction(nameof(Edit), new { id = model.ProductId });
		//}


		//public IActionResult EditMarketplaceProduct(int marketPlaceProductId)
		//{
		//	ViewBag.Action = ActionType.Edition;
		//	var model = _siteManager.GetMarketplaceProduct(marketPlaceProductId);
		//	SetViewDataMarketPlaces(model.ProductId);
		//	return PartialView("_EditMarketplaceProduct", model);
		//}

		//[HttpPost]
		//public async Task<IActionResult> EditMarketplaceProduct(MarketplaceProductModel model)
		//{
		//	if (ModelState.IsValid)
		//	{
		//		await _siteManager.UpdateMarketPlaceProductAsync(model);
		//		TempData["StatusMessage"] = Const.SuccessMessages.Saved;
		//	}
		//	else
		//	{
		//		TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
		//	}

		//	return RedirectToAction(nameof(Edit), new { id = model.ProductId });
		//}

		//[HttpPost]
		//public async Task<IActionResult> DeleteMarketPlaceProduct(int marketPlaceProductId)
		//{
		//	try
		//	{
		//		await _siteManager.DeleteMarketplaceProductAsync(marketPlaceProductId);
		//		return Ok();
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError(ex, $"[Site] Ошибка при удалении информации о маркетплейсее {marketPlaceProductId}");
		//	}

		//	return BadRequest();
		//}

		//private void SetViewDataMarketPlaces(int productId)
		//{
		//	var marketPlaces = _siteManager.GetAvailableMarketplaces(productId);
		//	ViewData["MarketPlaceId"] = marketPlaces
		//			.Select(mPlace => new SelectListItem { Text = mPlace.Name, Value = mPlace.MarketplaceId.ToString() });
		//}

		//private string GetView(string viewName)
		//{
		//	if (_currentSiteId == 1)
		//	{
		//		return $"/Views/SiteHome/{viewName}";
		//	}

		//	return $"/Views/GlobalSiteHome/{viewName}";
		//}

		//private string GetContentType(string path)
		//{
		//	var types = GetMimeTypes();
		//	var ext = Path.GetExtension(path).ToLowerInvariant();
		//	return types[ext];
		//}

		//private Dictionary<string, string> GetMimeTypes()
		//{
		//	return new Dictionary<string, string>
		//	{
		//		{".mp4", "video/mp4" },
		//		{".svg", "svg+xml" },
		//		{".png", "image/png"},
		//		{".jpg", "image/jpeg"},
		//		{".jpeg", "image/jpeg"},
		//		{".gif", "image/gif"}
		//	};
		//}

		//private void SetLocale()
		//{
		//	if (!_cache.TryGetValue(CacheKey, out var lang))
		//	{
		//		// Key not in cache, so get data.
		//		lang = _siteManager.GetLocale(_currentSiteId)
		//			.Substring(0, 2);

		//		// Set cache options.
		//		var cacheEntryOptions = new MemoryCacheEntryOptions()
		//			// Keep in cache for this time, reset time if accessed.
		//			.SetSlidingExpiration(TimeSpan.FromHours(12));

		//		// Save data in cache.
		//		_cache.Set(CacheKey, lang, cacheEntryOptions);
		//	}
		//	ViewData["Language"] = lang;
		//}
	}
}
