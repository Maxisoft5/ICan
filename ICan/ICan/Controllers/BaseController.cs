using AutoMapper;
using ICan.Business.Managers;
using ICan.Common.Models;
using ICan.Common.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICan.Controllers
{
	public class BaseController : Controller
	{
		protected readonly IMapper _mapper;

		protected readonly ILogger<BaseController> _logger;

		protected GlobalSettingManager _globalSettingManager { get; set; }

		protected UserManager<ApplicationUser> _userManager { get; }

		protected IConfiguration _configuration { get; }

		public BaseController(ILogger<BaseController> logger)
		{
			_logger = logger;
		}

		public BaseController(
				UserManager<ApplicationUser> userManager,
				ILogger<BaseController> logger) 
			: this(logger)
		{
			_userManager = userManager;
		}

		public BaseController(
				IMapper mapper,
				UserManager<ApplicationUser> userManager,
				ILogger<BaseController> logger) 
			: this(userManager, logger)
		{
			_mapper = mapper;
		}

		public BaseController(
				IMapper mapper,
				UserManager<ApplicationUser> userManager,
				ILogger<BaseController> logger,
				IConfiguration configuration) 
			: this(userManager, logger)
		{
			_mapper = mapper;
			_configuration = configuration;
		}

		public BaseController(
				UserManager<ApplicationUser> userManager,
				ILogger<BaseController> logger,
				IConfiguration configuration)
            : this(userManager, logger)
		{
			_configuration = configuration;
		}

		public IActionResult Error()
		{
			return View(new ErrorModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		public async Task<string> GetGlobalSetting(long id)
		{
			if (_globalSettingManager == null)
			{
				return null;
			}

			var glob = await _globalSettingManager.Get(id);
			return glob?.Content ?? string.Empty;
		}

		protected string GetErrors()
		{
			// https://www.jerriepelser.com/blog/validation-response-aspnet-core-webapi/
			StringBuilder errors = new StringBuilder();
			var modelState = ViewData.ModelState;
			modelState.Keys
				.SelectMany(key => modelState[key].Errors.Select(x => errors.Append(x.ErrorMessage)))
				.ToList();
			return errors.ToString();
		}

		protected void SetMonthsYears(bool currentSelected = false)
		{
			var thisYear = DateTime.Now.Year;
			int? thisMonth = currentSelected ? (int?)DateTime.Now.Month : null;
			var yearsList = Enumerable.Range(thisYear - 2, 3).ToList();
			var years = new List<SelectListItem>();
			yearsList.ForEach(t =>
				years.Add(new SelectListItem { Text = t.ToString(), Value = t.ToString(), Selected = t == thisYear }));
			ViewData["Years"] = years;
			ViewData["Months"] = Util.GetAllMonths(thisMonth);
		}

		protected static async Task<bool> SetFile(ILogger<BaseController> logger, IConfiguration configuration, string settingName, string newRequisites)
		{
			try
			{
				var filePath = configuration[settingName];
				using (StreamWriter outputFile = new StreamWriter(System.IO.File.Open(filePath, FileMode.Truncate), Encoding.GetEncoding(1251)))
				{
					await outputFile.WriteAsync(newRequisites);
					return true;
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Can't save file");
			}

			return false;
		}
	}
}