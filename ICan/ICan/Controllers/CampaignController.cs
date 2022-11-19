using ICan.Business.Managers;
using ICan.Common;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ICan.Controllers
{
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class CampaignController : BaseController
	{
		private readonly CampaignManager _campaignManager;

		public CampaignController(ILogger<BaseController> logger, CampaignManager campaignManager) : base(logger)
		{
			_campaignManager = campaignManager;
		}

		public IActionResult Index()
		{
			var campaings = _campaignManager.GetCampaigns();
			return View(campaings);
		}

		public async Task<IActionResult> ExportContacts()
		{
			try
			{
				var result = await _campaignManager.ExportContacts();
				TempData["StatusMessage"] = $"Экспортировано успешно контактов: {result}";
			}
			catch (UserException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				TempData["ErrorMessage"] = "Не удалось экспортирова контакты";
			}
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(CampaignModel campaing)
		{
			try
			{
				if (campaing.CampaignType == CampaignType.None)
					throw new UserException("Выберите тип кампании");

				await _campaignManager.AddCampaign(campaing);
				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				return RedirectToAction(nameof(Index));
			}
			catch (UserException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSave;
			}
			return View(campaing);
		}

		public async Task<IActionResult> Edit(int id)
		{
			var campaing = await _campaignManager.Get(id);
			return View(campaing);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(CampaignModel campaing)
		{
			try
			{
				await _campaignManager.Edit(campaing);
				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSave;
			}
			return View(campaing);
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _campaignManager.Delete(id);
				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				TempData["ErrorMessage"] = Const.ErrorMessages.CantDeleteForUser;
			}
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> SendCampaing(int id)
		{
			try
			{
				await _campaignManager.SendCampaign(id);
				TempData["StatusMessage"] = "Рассылка успешно отправлена";
			}
			catch (UserException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				TempData["ErrorMessage"] = "При отправке рассылки произошла ошибка";
			}

			return RedirectToAction(nameof(Index));
		}
		
		[HttpPost]
		public async Task<IActionResult> PrepareCampaign(int id)
		{
			try
			{
				await _campaignManager.PrepareCampaign(id);
				TempData["StatusMessage"] = "Рассылка успешно подготовлена";
			}
			catch (UserException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				TempData["ErrorMessage"] = "При поготовке рассылки произошла ошибка";
			}

			return RedirectToAction(nameof(Index));
		}


		public IActionResult AddImage()
		{
			return PartialView();
		}

		[HttpPost]
		public async Task<string> AddImage(IFormFile file, int campaignId)
		{
			return await _campaignManager.AddImage(file, campaignId);
		}

		public async Task<IActionResult> Details(int id)
		{
			var model = await _campaignManager.Get(id);
			return View(model);
		}
	}
}
