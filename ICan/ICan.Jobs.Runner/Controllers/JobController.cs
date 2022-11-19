using Hangfire;
using ICan.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ICan.Jobs.Runner.Controllers
{
	[Route("[controller]")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class JobController : Controller
	{
		private readonly IRecurringJobManager _recurringJobManager;
		private readonly ILogger<JobController> _logger;

		public JobController(IRecurringJobManager recurringJobManager, ILogger<JobController> logger)
		{
			_logger = logger;
			_recurringJobManager = recurringJobManager;
		}
		
		[HttpPost("RunOzonParseJob")]
		public async Task<IActionResult> RunOzonParseJob()
		{
			try
			{
				_recurringJobManager.Trigger(Const.JobName.OzonApiPriceImportJob);
				return Ok();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Site] Ошибка при запуске задачи получения цен по апи из Озон");
				TempData["ErrorMessage"] = Const.ErrorMessages.CantStart;
			}
			return BadRequest();

		}

		[HttpPost("RunWbParseJob")]
		public async Task<IActionResult> RunWbParseJob()
		{
			try
			{
				_recurringJobManager.Trigger(Const.JobName.WbMarketplaceParse);
				return Ok();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Site] Ошибка при запуске задачи  парсинга сайта WB");				
			}
			return BadRequest();
		}
	}
}
