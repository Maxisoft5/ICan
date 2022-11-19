using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Common.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ICan.Common.Models.Opt;
using ICan.Common;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class EventController : BaseController
	{
		private readonly EventManager _eventManager;

		public EventController(EventManager eventManager,
			UserManager<ApplicationUser> userManager,
			ILogger<BaseController> logger) : base(userManager, logger)
		{
			_eventManager = eventManager;
		}

		public async Task<IActionResult> Index()
		{
			IEnumerable<EventModel> events = await _eventManager.Get();
			return View(events);
		}

		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}
			ViewData["ActionType"] = ActionType.Details;

			EventModel model = await _eventManager.Get(id);

			if (model == null)
			{
				return NotFound();
			}

			return View("Edit", model);
		}

		public IActionResult Create()
		{
			ViewBag.Action = ActionType.Creation;
			return View();
		}

		// POST: Events/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("EventId,Name,StartDate,EndDate,Enabled,DiscountPercent,Description")]
				EventModel eventModel)
		{
			if (ModelState.IsValid)
			{
				var rangeEvents = _eventManager.CheckEvent(eventModel);
				if (rangeEvents != null && rangeEvents.Any())
				{
					ModelState.AddModelError("",
						string.Format("Существуют пересекающиеся акции: {0}",
						string.Join(", ", rangeEvents.Select(t => t.Name))));
					return View(eventModel);
				}
				await _eventManager.Add(eventModel);
				return RedirectToAction(nameof(Index));
			}
			return View(eventModel);
		}

		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			ViewData["ActionType"] = ActionType.Edition;
			var model = await _eventManager.Get(id);
			if (model == null)
			{
				return NotFound();
			}

			return View("Edit", model);
		}

		// POST: Events/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id,
			[Bind("EventId,Name,StartDate,EndDate,Enabled,DiscountPercent,Description")]
			EventModel eventModel)
		{
			if (id != eventModel.EventId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					var rangeEvents = _eventManager.CheckEvent(eventModel);
					if (rangeEvents != null && rangeEvents.Any())
					{
						ModelState.AddModelError("",
							string.Format("Существуют пересекающиеся акции: {0}",
							string.Join(", ", rangeEvents.Select(t => t.Name))));
						ViewData["ActionType"] = ActionType.Edition;
						return View(eventModel);
					}
					await _eventManager.Update(eventModel);
					return RedirectToAction(nameof(Index));
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, string.Format(Const.ErrorMessages.CantSave,
						"Edit", this.GetType().ToString(), eventModel.EventId));
				}
			}
			ViewData["ActionType"] = ActionType.Edition;
			return View(eventModel);
		}


		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			await _eventManager.MarkIsDeleted(id);
			return RedirectToAction(nameof(Index));
		}
	}
}
