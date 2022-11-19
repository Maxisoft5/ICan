using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ICan.Common.Models.Opt;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,Operator")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class WarehouseTypeController : BaseController
	{
		private readonly CommonManager<OptWarehouseType> _manager;

		public WarehouseTypeController(CommonManager<OptWarehouseType> manager,
			ILogger<BaseController> logger) : base(logger)
		{
			_manager = manager;
		}

		public IActionResult Index()
		{
			var list = _manager.Get<WarehouseTypeModel>("Counterparty");
			return View(list);
		}
	}
}
