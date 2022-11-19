using Hangfire;
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
using System.Security.Claims;
using System.Threading.Tasks;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,ContentMan")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class SiteHomeController : BaseController
	{
		private readonly SiteManager _siteManager;
		private readonly MarketplaceManager _marketplaceManager;
		private readonly CommonManager<OptTag> _tagManager;
		private readonly int _currentSiteId;
		private readonly string _bucketUrl;
		private readonly IMemoryCache _cache;
		private static readonly string CacheKey = "Locale";
		private static HttpClient Client = new HttpClient();

		public SiteHomeController(
			IConfiguration configuration,
			UserManager<ApplicationUser> userManager,
			MarketplaceManager marketplaceManager,
			CommonManager<OptTag> tagManager,
			ILogger<BaseController> logger,
			SiteManager siteManager,
			IOptions<CloudConfiguration> cloudConfig,
			IMemoryCache memoryCache)
			: base(userManager, logger, configuration)
		{
			_siteManager = siteManager;
			_marketplaceManager = marketplaceManager;
			_tagManager = tagManager;
			_currentSiteId = int.Parse(configuration["Settings:CurrentSiteId"]);
			_bucketUrl = cloudConfig.Value.BucketUrl;
			_cache = memoryCache;
		}

		[AllowAnonymous]
		[Route("/")]
		[Route("/category/{*categoryFilter}")]
		[Route("/SiteHome")]
		[Route("/inst")]
		[Route("/SiteHome/Index")]
		//[ResponseCache(VaryByQueryKeys = new string[] { "categoryFilter", "filter", "tag", "notebookLang" }, Duration = 60 * 60 * 2, Location = ResponseCacheLocation.Any)]
		public IActionResult Index(ClientFilter clientfilter)
		{
			ViewData["ShowFilter"] = true;
			ViewData["SelectedFilter"] = clientfilter.Filter;
			ViewData["SelectedTag"] = clientfilter.Tag;
			ViewData["SelectedLang"] = clientfilter.NotebookLang;
			SetLocale();

			var products = _siteManager.GetAll(_currentSiteId, clientfilter);
			var view = GetView("Index.cshtml");
			return View(view, products);
		}

		[AllowAnonymous]
		[Route("/sitedata/{fileName}")]
		public async Task<IActionResult> SiteData(string fileName)
		{
			try
			{
				_logger.LogWarning("[SiteData][SiteHome]");
				var stream = await Client.GetStreamAsync($"{_bucketUrl}/{fileName}");
				_logger.LogWarning("[SiteData][SiteHome] success");
				return new FileStreamResult(stream, GetContentType(fileName));
			}
			catch (Exception ex)
			{
				_logger.LogWarning($"[SiteData][SiteHome] error {ex}");
				return NotFound();
			}
		}

		[AllowAnonymous]
		[Route("/AboutUs")]
		[Route("/SiteHome/AboutUs")]
		public IActionResult AboutUs()
		{
			SetLocale();
			return View();
		}

		[AllowAnonymous]
		[Route("/error")]
		[Route("/SiteHome/error")]
		public IActionResult ErrorPage()
		{
			return View(GetView("ErrorPage.cshtml"));
		}

		public IActionResult UploadMarketPlaceFile() => View("_AddMarketPlaceFile");

		public async Task<IActionResult> ProductList()
		{
			var products = await _siteManager.GetSiteProductsAsync();
			return View(products);
		}

		[Route("/Contacts")]
		[Route("/SiteHome/Contacts")]
		[AllowAnonymous]
		public IActionResult Contacts()
		{
			SetLocale();
			return View();
		}

		[Route("/Notebook/{id}")]
		[Route("/SiteHome/Notebook/{id}")]
		[AllowAnonymous]
		public async Task<IActionResult> Notebook(int id)
		{
			var model = await _siteManager.GetAsync(_currentSiteId, id);
			SetLocale();
			if (model != null)
			{
				return View(GetView("Notebook.cshtml"), model);
			}

			return ErrorPage();
		}

		[HttpGet("[controller]/Edit")]
		public async Task<IActionResult> Edit(int id)
		{
			var model = await _siteManager.GetAsync(_currentSiteId, id, true);
			return View("Edit", model);
		}

		[HttpPost("[controller]/Edit")]
		public async Task<IActionResult> Edit(SiteProductModel model, IFormFile videoFileName)
		{
			if (ModelState.IsValid)
			{
				try
				{
					await _siteManager.UpdateAsync(model, videoFileName);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "[Site] Ошибка при сохранении тетради для сайта");
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
				}
			}
			else
			{
				TempData["ErrorMessage"] = $"Невозможно сохранить информацю {GetErrors()}";
			}
			return RedirectToAction(nameof(ProductList));
		}

		[HttpPost("[controller]/RunOzonParseJob")]
		public async Task<IActionResult> RunOzonParseJob()
		{
			try
			{
				var userId = _userManager.GetUserId(HttpContext.User);
				await _siteManager.RunJob(userId, _configuration["Jobs:OzonJob"]);
				TempData["StatusMessage"] = Const.SuccessMessages.Started;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Site] Ошибка при запуске задачи получения цен по апи из Озон");
				TempData["ErrorMessage"] = Const.ErrorMessages.CantStart;
			}

			return RedirectToAction(nameof(ProductList));
		}

		[HttpPost("[controller]/RunWbParseJob")]
		public async Task<IActionResult> RunWbParseJob()
		{
			try
			{
				var userId = _userManager.GetUserId(HttpContext.User);
				await _siteManager.RunJob(userId, _configuration["Jobs:WbJob"]);
				TempData["StatusMessage"] = Const.SuccessMessages.Started;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Site] Ошибка при запуске задачи  парсинга сайта WB");
				TempData["ErrorMessage"] = Const.ErrorMessages.CantStart;
			}

			return RedirectToAction(nameof(ProductList));
		}

		[HttpPost]
		public async Task<IActionResult> UploadMarketPlaceFile(IFormFile marketPlaceFile)
		{
			try
			{
				await _siteManager.ParseFileAsync(marketPlaceFile);
				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
			}
			catch (UserException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Site] Ошибка при загрузке файла");
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
			}
			return RedirectToAction(nameof(ProductList));
		}

		public IActionResult AddImage(int productId)
		{
			bool catalogImageExists = _siteManager.CatalogImageExists(productId);
			var model = new ProductImageModel
			{
				ProductId = productId,
				ImageType = ProductImageType.Catalog,
			};

			var list = new List<SelectListItem>();
			if (!catalogImageExists)
			{
				list.Add(
				new SelectListItem
				{
					Text = ProductImageType.Catalog.GetDisplayName(),
					Value = ((int)ProductImageType.Catalog).ToString(),
				});
			}
			list.Add(new SelectListItem
			{
				Text = ProductImageType.SmallGallery.GetDisplayName(),
				Value = ((int)ProductImageType.SmallGallery).ToString(),
			});
			list.Add(new SelectListItem
			{
				Text = ProductImageType.BigGallery.GetDisplayName(),
				Value = ((int)ProductImageType.BigGallery).ToString()
			});
			list.Add(new SelectListItem
			{
				Text = ProductImageType.TextImage.GetDisplayName(),
				Value = ((int)ProductImageType.TextImage).ToString(),
			});
			list.Add(new SelectListItem
			{
				Text = ProductImageType.RichContent.GetDisplayName(),
				Value = ((int)ProductImageType.RichContent).ToString(),
			});
			list.Add(new SelectListItem
			{
				Text = ProductImageType.MobileCatalog.GetDisplayName(),
				Value = ((int)ProductImageType.MobileCatalog).ToString(),
			});
			list.Add(new SelectListItem
			{
				Text = ProductImageType.AliExpress.GetDisplayName(),
				Value = ((int)ProductImageType.AliExpress).ToString(),
			});
			ViewData["ImageTypes"] = list;
			return PartialView(model);
		}

		[HttpPost]
		public async Task<IActionResult> AddImage(ProductImageModel model, IFormFile image)
		{
			try
			{
				bool catalogImageExists = _siteManager.CatalogImageExists(model.ProductId);
				if (catalogImageExists && model.ImageType == ProductImageType.Catalog)
				{
					TempData["ErrorMessage"] = "Невозможно добавить для тетради вторую иллюстрацию типа \"Каталог\"";
				}
				else if (ModelState.IsValid)
				{
					await _siteManager.AddImageAsync(model, image);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Site] Ошибка при сохранении иллюстрации");
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
			}

			return RedirectToAction(nameof(Edit), "SiteHome", new { id = model.ProductId }, "image-info");
		}

		[HttpPost]
		public async Task<IActionResult> DeleteImage(int imageId, int productId)
		{
			try
			{
				await _siteManager.DeleteImage(imageId);

				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				return RedirectToAction(nameof(Edit), new { id = productId });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Site] Ошибка при сохранении иллюстрации");
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
			}

			return BadRequest();
		}

		[HttpPost]
		public async Task<IActionResult> DeleteVideo(int productId)
		{
			try
			{
				await _siteManager.DeleteVideo(productId);

				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				return Ok();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Site] Ошибка при сохранении видео");
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
			}

			return BadRequest();
		}

		public async Task<IActionResult> AddTag(int productId)
		{
			var model = new ProductTagModel
			{
				ProductId = productId,
			};
			var tags = await _tagManager.GetAsync();
			var existingTags = _siteManager.GetProductTagIds(productId);
			var availableTags = tags.Where(tag => !existingTags.Contains(tag.TagId)).Select(tag => new SelectListItem
			{
				Text = tag.TagName,
				Value = tag.TagId.ToString(),
			});
			ViewData["TagId"] = availableTags;
			ViewData["CanAddMore"] = availableTags.Any();
			return PartialView(model);
		}

		[HttpPost]
		public async Task<IActionResult> AddTag(ProductTagModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					await _siteManager.AddTagAsync(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Site] Ошибка при сохранении иллюстрации");
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
			}

			return RedirectToAction(nameof(Edit), "SiteHome", new { id = model.ProductId });
		}

		[HttpPost]
		public async Task<IActionResult> DeleteTag(int productTagId, int productId)
		{
			try
			{
				if (ModelState.IsValid)
				{
					await _siteManager.DeleteTag(productTagId);
				}

				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				return RedirectToAction(nameof(Edit), new { id = productId });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Site] Ошибка при сохранении иллюстрации");
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
			}
			return BadRequest();
		}

		public IActionResult AddMarketplaceProduct(int productId)
		{
			var model = new MarketplaceProductModel
			{
				ProductId = productId,
			};

			ViewBag.Action = ActionType.Creation;
			SetViewDataMarketPlaces(productId);
			return PartialView("_EditMarketplaceProduct", model);
		}

		[HttpPost]
		public async Task<IActionResult> AddMarketplaceProduct(MarketplaceProductModel model)
		{
			if (ModelState.IsValid)
			{
				await _siteManager.AddMarketPlaceProductAsync(model);
				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
			}
			else
			{
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
			}

			return RedirectToAction(nameof(Edit), new { id = model.ProductId });
		}


		public IActionResult EditMarketplaceProduct(int marketPlaceProductId)
		{
			ViewBag.Action = ActionType.Edition;
			var model = _siteManager.GetMarketplaceProduct(marketPlaceProductId);
			SetViewDataMarketPlaces(model.ProductId);
			return PartialView("_EditMarketplaceProduct", model);
		}

		[HttpPost]
		public async Task<IActionResult> EditMarketplaceProduct(MarketplaceProductModel model)
		{
			if (ModelState.IsValid)
			{
				await _siteManager.UpdateMarketPlaceProductAsync(model);
				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
			}
			else
			{
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
			}

			return RedirectToAction(nameof(Edit), new { id = model.ProductId });
		}

		[HttpPost]
		public async Task<IActionResult> DeleteMarketPlaceProduct(int marketPlaceProductId)
		{
			try
			{
				await _siteManager.DeleteMarketplaceProductAsync(marketPlaceProductId);
				return Ok();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"[Site] Ошибка при удалении информации о маркетплейсее {marketPlaceProductId}");
			}

			return BadRequest();
		}

		private void SetViewDataMarketPlaces(int productId)
		{
			var marketPlaces = _siteManager.GetAvailableMarketplaces(productId);
			ViewData["MarketPlaceId"] = marketPlaces
					.Select(mPlace => new SelectListItem { Text = mPlace.Name, Value = mPlace.MarketplaceId.ToString() });
		}

		private string GetView(string viewName)
		{
			if (_currentSiteId == 1)
			{
				return $"/Views/SiteHome/{viewName}";
			}

			return $"/Views/GlobalSiteHome/{viewName}";
		}

		private string GetContentType(string path)
		{
			var types = GetMimeTypes();
			var ext = Path.GetExtension(path).ToLowerInvariant();
			return types[ext];
		}

		private Dictionary<string, string> GetMimeTypes()
		{
			return new Dictionary<string, string>
			{
				{".mp4", "video/mp4" },
				{".svg", "svg+xml" },
				{".png", "image/png"},
				{".jpg", "image/jpeg"},
				{".jpeg", "image/jpeg"},
				{".gif", "image/gif"}
			};
		}

		private void SetLocale()
		{
			if (!_cache.TryGetValue(CacheKey, out var lang))
			{
				// Key not in cache, so get data.
				lang = _siteManager.GetLocale(_currentSiteId)
					.Substring(0, 2);

				// Set cache options.
				var cacheEntryOptions = new MemoryCacheEntryOptions()
					// Keep in cache for this time, reset time if accessed.
					.SetSlidingExpiration(TimeSpan.FromHours(12));

				// Save data in cache.
				_cache.Set(CacheKey, lang, cacheEntryOptions);
			}
			ViewData["Language"] = lang;
		}
	}
}
