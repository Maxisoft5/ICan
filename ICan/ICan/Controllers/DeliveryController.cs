using ICan.Extensions;
using ICan.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ICan.Common;
using ICan.Common.Utils;
using ICan.Business.Managers;

namespace ICan.Controllers
{
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class DeliveryController : BaseController
	{
		public DeliveryController(
			ILogger<BaseController> logger,
			 GlobalSettingManager globalSettingManager) : base(logger)
		{
			_globalSettingManager = globalSettingManager;
		}

		// GET: Delivery
		public async Task<ActionResult> Index()
		{
			var deliveryInfo = await GetGlobalSetting(Const.DeliveryInfoId);
			return View("Index", deliveryInfo);
		}
	}
}