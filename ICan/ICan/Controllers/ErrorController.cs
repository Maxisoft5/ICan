using ICan.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ICan.Controllers
{
	public class ErrorController : Controller
	{
		[AllowAnonymous]
		public IActionResult Index()
		{
			return View("~/Views/Shared/Error.cshtml"
			, new ErrorModel
			{ RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });

		}

		public IActionResult NoRights()
		{
			return View("~/Views/Shared/NoRights.cshtml");
			/*
			, new ErrorViewModel
				{ RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
				*/
		}


		[AllowAnonymous]
		public IActionResult NotFoundError()
		{

			return View("~/Views/Shared/NotFoundError.cshtml");
			/*
			, new ErrorViewModel
				{ RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
				*/
		}
	}
}