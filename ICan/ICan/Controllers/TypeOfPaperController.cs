using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models.Exceptions;
using ICan.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ICan.Common.Models.Opt;
using ICan.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,Assembler")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class TypeOfPaperController : BaseController
	{
		private readonly TypeOfPaperManager _typeOfPaperManager;

		public TypeOfPaperController(ILogger<BaseController> logger, TypeOfPaperManager typeOfPaperManager): base(logger)
		{
			_typeOfPaperManager = typeOfPaperManager;
		}

		public async Task<IActionResult> Index()
		{
			var typesOfPaper = await _typeOfPaperManager.GetTypes();
			return View(typesOfPaper);
		}

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(TypeOfPaperModel typeOfPaperModel)
		{
			try
			{
				await _typeOfPaperManager.Create(typeOfPaperModel);
				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				return RedirectToAction("Index");
			}
			catch(UserException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			return View();
		}


		public async Task<IActionResult> Edit(int id)
		{
			try
			{
				var model = await _typeOfPaperManager.GetTypeById(id);
				return View(model);
			}
			catch(UserException ex)
			{
				TempData["ErrorMessage"] = ex.Message;				
			}
			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> Edit(TypeOfPaperModel typeOfPaper)
		{
			try
			{
				await _typeOfPaperManager.Update(typeOfPaper);
				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				return RedirectToAction("Index");
			}
			catch(UserException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
				_logger.LogError(ex.Message);
			}
			return View(typeOfPaper);
		}


		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _typeOfPaperManager.Delete(id);
				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
				return RedirectToAction("Index");
			}
			catch (UserException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (DbUpdateException ex)
			{
				_logger.LogError(ex.Message);
				TempData["ErrorMessage"] = Const.ErrorMessages. CantDeleteTypeOfPaper;
			}
			catch(Exception ex)
			{
				TempData["ErrorMessage"] = Const.ErrorMessages.CantDeleteForUser;
				_logger.LogError(ex.Message);
			}
			return RedirectToAction("Index");
		}
	}
}
