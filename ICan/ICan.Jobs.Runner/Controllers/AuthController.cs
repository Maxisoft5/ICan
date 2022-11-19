using ICan.Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ICan.Jobs.Runner.Controllers
{
	public class AuthController : Controller
	{
		private readonly SignInManager<ApplicationUser> _singInManager;
		
		public AuthController(SignInManager<ApplicationUser> signInManager)
		{
			_singInManager = signInManager;
		}

		public IActionResult Index()
		{
			return View("~/Pages/Index.cshtml");
		}

		[HttpPost]
		public async Task<IActionResult> Login(AuthModel model)
		{
			if (ModelState.IsValid)
			{
				var login = await _singInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);
				if(login.Succeeded)
					return RedirectPermanent("/hangfire");
			}
			TempData["ErrorMessage"] = "Не верная почта или пароль";
			return View("~/Pages/Index.cshtml");
		}
	}
}
