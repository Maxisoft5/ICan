using ICan.Extensions;
using ICan.Business.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ICan.Controllers
{
	[Authorize()]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class OrderSizeDiscountController : BaseController
	{
		private readonly OrderSizeDiscountManager _orderSizeDiscountManager;
		public OrderSizeDiscountController( 
			ILogger<BaseController> logger,
			OrderSizeDiscountManager orderSizeDiscountManager) : base(logger)
		{
			_orderSizeDiscountManager = orderSizeDiscountManager;
		}

		[AllowAnonymous]
		public async Task<IActionResult> Get()
		{
			var orderSizes = await _orderSizeDiscountManager.Get();
			return Ok(orderSizes);
		}
	}
}
