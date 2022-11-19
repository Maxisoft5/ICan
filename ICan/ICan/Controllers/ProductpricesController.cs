using ICan.Extensions;
using ICan.Business.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,Operator")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class ProductpricesController : BaseController
	{
		private readonly ProductManager _productManager;
		public ProductpricesController(
			ProductManager productManager,
			ILogger<BaseController> logger) : base(logger)
		{
			_productManager = productManager;
		}

		public IActionResult Archive(int id)
		{
			var product = _productManager.GetProductWithPrices(id);
			var isAdmin = HttpContext.User.IsInRole("Admin");
			ViewData["IsAdmin"] = isAdmin;
			ViewData["ProductTitle"] = $"{product.DisplayName}  [{product.ProductSeriesName}]";

			var prices = product.ProductPrices.OrderBy(price => price.DateStart);
			return View("Index", prices);
		}
	}
}
