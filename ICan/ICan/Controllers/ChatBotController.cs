using ICan.Business.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NUglify;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICan.Controllers
{
	public class ChatBotController : BaseController
	{
		private readonly SiteManager _siteManager;
		private readonly ProductManager _productManager;


		public ChatBotController(
			ILogger<BaseController> logger,
			ProductManager productManager,
			SiteManager siteManager)
			: base(logger)
		{
			_siteManager = siteManager;
			_productManager = productManager;
		}

		[HttpGet("/chatbot/notebook/{id}")]
		[AllowAnonymous]
		public async Task<IActionResult> Notebook(int id)
		{
			var currentSiteId = 1;
			var host = $"{Request.Scheme}://{Request.Host}/";
			var model = await _siteManager.GetAsync(currentSiteId, id);
			return Ok(new
			{
				Name = model.SiteName,
				CoverUrl = model.CoverCatalogImageFullPath,
				ProductUrl =$"{host}sitehome/notebook/{id}",
				VideoUrl =  model.VideoFileFullPath,
				Characteristics = model.BotInformation,
				Description = model.BotDescription,
			});
		}


		[HttpGet("/chatbot/copyinfo")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> CopyInfo()
		{
			try
			{
				await _siteManager.SetBotDescription();
				return Ok();
			}
			catch (Exception ex) {
				_logger.LogError(ex, "[chatbot][copyinfo]");
				return BadRequest();
			}
		}
	}
}
