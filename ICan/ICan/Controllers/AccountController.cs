using ICan.Common.Models;
using ICan.Business.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using ICan.Common.Models.AccountViewModels;
using ICan.Common;
using ICan.Data.Context;
using ICan.Extensions;

namespace ICan.Controllers
{
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class AccountController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly IEmailSender _emailSender;
		private readonly ILogger _logger;

		private readonly ApplicationDbContext _context;
		private readonly IConfiguration _configuration;

		private readonly string GoogleUrl;
		private readonly string Secret;
		private readonly string Public;
		private readonly bool UseCaptcha;

		public AccountController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			IEmailSender emailSender,
			ILogger<BaseController> logger,
			 ApplicationDbContext context,
				IConfiguration configuration)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_emailSender = emailSender;
			_logger = logger;
			_context = context;
			_configuration = configuration;
			Secret = _configuration["Settings:Captcha:Secret"];
			Public = _configuration["Settings:Captcha:Public"];
			GoogleUrl = _configuration["Settings:Captcha:GoogleUrl"];
			bool.TryParse(_configuration["Settings:Captcha:Enabled"]?.ToString(), out UseCaptcha);
			_logger.LogWarning($"Secret {Secret} Public {Public} GoogleUrl {GoogleUrl} UseCaptcha {UseCaptcha}");
		}

		[TempData]
		public string ErrorMessage { get; set; }

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> Login(string returnUrl = null)
		{
			// Clear the existing external cookie to ensure a clean login process
			await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
			ViewData["ReturnUrl"] = returnUrl;
			ViewData["UseCaptcha"] = UseCaptcha;
			ViewData["Public"] = Public;
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;
			ViewData["Public"] = Public;
			ViewData["UseCaptcha"] = UseCaptcha;

			if (ModelState.IsValid)
			{
				if (UseCaptcha)
				{
					var captcha = HttpContext.Request.Form["g-recaptcha-response"];
					var validationResult = await ValidateCaptcha(captcha);
					if (!validationResult)
					{
						ModelState.AddModelError(string.Empty, "Google считает, что вы робот");
						return View(model);
					}
				}
				try
				{               // This doesn't count login failures towards account lockout
								// To enable password failures to trigger account lockout, set lockoutOnFailure: true
					var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent:false, lockoutOnFailure: false);

					if (result.IsLockedOut)
					{
						_logger.LogWarning("User account locked out.");
						return RedirectToAction(nameof(Lockout));
					}

					if (result.Succeeded)
					{
						_logger.LogInformation("User logged in.");
						return RedirectToLocal(returnUrl);
					}
					else
					{
						ModelState.AddModelError(string.Empty, "Неверная почта или пароль");
						return View(model);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"login failed {model.Email}");
					ModelState.AddModelError(string.Empty, "Неверная почта или пароль");
					return View(model);
				}

			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
		{
			// Ensure the user has gone through the username & password screen first
			var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

			if (user == null)
			{
				throw new ApplicationException($"Unable to load two-factor authentication user.");
			}

			var model = new LoginWith2faViewModel { RememberMe = rememberMe };
			ViewData["ReturnUrl"] = returnUrl;

			return View(model);
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string returnUrl = null)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
			if (user == null)
			{
				throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

			var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

			if (result.Succeeded)
			{
				_logger.LogInformation("User with ID {UserId} logged in with 2fa.", user.Id);
				return RedirectToLocal(returnUrl);
			}
			else if (result.IsLockedOut)
			{
				_logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
				return RedirectToAction(nameof(Lockout));
			}
			else
			{
				_logger.LogWarning("Invalid authenticator code entered for user with ID {UserId}.", user.Id);
				ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
				return View();
			}
		}
		 

		[HttpGet]
		[AllowAnonymous]
		public IActionResult Lockout()
		{
			return View();
		}

		[HttpGet]
		[AllowAnonymous]
		public IActionResult Register(string returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;
			ViewData["UseCaptcha"] = UseCaptcha;
			ViewData["Public"] = Public;
			return View();
		}


		public async Task<bool> ValidateCaptcha(string response)
		{
			var httpClient = new HttpClient();
			var url = string.Format(GoogleUrl, Secret, response);
			var res = await httpClient.GetAsync(url);

			if (res.StatusCode != HttpStatusCode.OK)
			{
				_logger.LogError($"Error while sending request to ReCaptcha {response}");
				return false;
			}

			string JSONres = res.Content.ReadAsStringAsync().Result;
			dynamic JSONdata = JObject.Parse(JSONres);
			bool.TryParse(JSONdata?.success?.ToString(), out bool success);
			return success;
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;
			ViewData["Public"] = Public;
			ViewData["UseCaptcha"] = UseCaptcha;
			if (ModelState.IsValid)
			{
				if (UseCaptcha)
				{
					var captcha = HttpContext.Request.Form["g-recaptcha-response"];
					var validationResult = await ValidateCaptcha(captcha);
					if (!validationResult)
					{
						ModelState.AddModelError(string.Empty, "Google считает, что вы робот");
						return View(model);
					}
				}
				var user = new ApplicationUser
				{
					UserName = model.Email,
					Email = model.Email,
					PhoneNumber = model.Phone,
					FirstName = model.FirstName,
					LastName = model.LastName,
					IsClient = true
				};
				var result = await _userManager.CreateAsync(user, model.Password);
				if (result.Succeeded)
				{
					_context.SaveChanges();
					_logger.LogInformation("User created a new account with password.");

					var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
					var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
					bool.TryParse(_configuration["Settings:Mail:AllowSendingMail"], out var canSend);
					if (canSend)
						_emailSender.SendEmailConfirmation(model.Email, callbackUrl);

					await _signInManager.SignInAsync(user, isPersistent: false);

					

					return RedirectToLocal(returnUrl);
				}
				AddErrors(result);
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]

		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			_logger.LogInformation("User logged out.");
			return RedirectToAction(nameof(ProductController.Index), "Product");
		}

		//[AllowAnonymous]
		//[HttpPost]
		//[ActionName("Logout")]
		//public async Task<IActionResult> LogoutRedirect()
		//{

		//	return RedirectToAction(nameof(Login), "Account");
		//}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public IActionResult ExternalLogin(string provider, string returnUrl = null)
		{
			// Request a redirect to the external login provider.
			var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
			var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return Challenge(properties, provider);
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
		{
			if (remoteError != null)
			{
				ErrorMessage = $"Error from external provider: {remoteError}";
				return RedirectToAction(nameof(Login));
			}
			var info = await _signInManager.GetExternalLoginInfoAsync();
			if (info == null)
			{
				return RedirectToAction(nameof(Login));
			}

			// Sign in the user with this external login provider if the user already has a login.
			var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
			if (result.Succeeded)
			{
				_logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
				return RedirectToLocal(returnUrl);
			}
			if (result.IsLockedOut)
			{
				return RedirectToAction(nameof(Lockout));
			}
			else
			{
				// If the user does not have an account, then ask the user to create an account.
				ViewData["ReturnUrl"] = returnUrl;
				ViewData["LoginProvider"] = info.LoginProvider;
				var email = info.Principal.FindFirstValue(ClaimTypes.Email);
				return View("ExternalLogin", new ExternalLoginViewModel { Email = email });
			}
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null)
		{
			if (ModelState.IsValid)
			{
				// Get the information about the user from the external login provider
				var info = await _signInManager.GetExternalLoginInfoAsync();
				if (info == null)
				{
					throw new ApplicationException("Error loading external login information during confirmation.");
				}
				var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
				var result = await _userManager.CreateAsync(user);
				if (result.Succeeded)
				{
					result = await _userManager.AddLoginAsync(user, info);
					if (result.Succeeded)
					{
						await _signInManager.SignInAsync(user, isPersistent: false);
						_logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
						return RedirectToLocal(returnUrl);
					}
				}
				AddErrors(result);
			}

			ViewData["ReturnUrl"] = returnUrl;
			return View(nameof(ExternalLogin), model);
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> ConfirmEmail(string userId, string code)
		{
			if (userId == null || code == null)
			{
				return RedirectToAction(nameof(ProductController.Index), "Product");
			}
			var user = await _userManager.FindByIdAsync(userId);

			if (user == null)
			{
				throw new ApplicationException($"Unable to load user with ID '{userId}'.");
			}
			var model = new RegisterViewModel
			{
				FirstName = user.FirstName,
				LastName = user.LastName,
				Email = user.Email,
				Phone = user.PhoneNumber
			};
			return View(model);
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> ConfirmEmail(string userId, string code, RegisterViewModel model)
		{
			if (userId == null || code == null)
			{
				return RedirectToAction(nameof(ProductController.Index), "Product");
			}
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByIdAsync(userId);
				if (user == null)
				{
					throw new ApplicationException($"Unable to load user with ID '{userId}'.");
				}
				var result = await _userManager.ConfirmEmailAsync(user, code);
				if (!result.Succeeded)
				{
					TempData["ErrorMessage"] = "Невозможно подтвердить Email. Попросите выслать письмо о подвтерждении повторно";
					return View(model);
				}

				user.FirstName = model.FirstName;
				user.LastName = model.LastName;
				user.PhoneNumber = model.Phone;
				await _userManager.ChangePasswordAsync(user, Const.TemporaryPassword, model.Password);
				await _context.SaveChangesAsync();
				await _signInManager.SignInAsync(user, isPersistent: false);
				return RedirectToAction("Index", "Product");
			}
			return View(model);
		}



		[HttpGet]
		[AllowAnonymous]
		public IActionResult ForgotPassword()
		{
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(model.Email?.ToUpper());
				// надеюсь, временно, до того, как разберусь с подтверждением пароля
				//	if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
				if (user == null)
				{
					// Don't reveal that the user does not exist or is not confirmed
					return RedirectToAction(nameof(ForgotPasswordConfirmation));
				}

				// For more information on how to enable account confirmation and password reset please
				// visit https://go.microsoft.com/fwlink/?LinkID=532713
				var code = await _userManager.GeneratePasswordResetTokenAsync(user);
				var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);
				_emailSender.SendEmail(model.Email, "Сброс пароля",
					$"Пожалуйста, сбросьте свой пароль, перейдя по <a href='{callbackUrl}'>ссылке</a>");
				return RedirectToAction(nameof(ForgotPasswordConfirmation));
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		[HttpGet]
		[AllowAnonymous]
		public IActionResult ForgotPasswordConfirmation()
		{
			return View();
		}

		[HttpGet]
		[AllowAnonymous]
		public IActionResult ResetPassword(string code = null, string userId = null)
		{
			if (code == null)
			{
				throw new ApplicationException("A code must be supplied for password reset.");
			}
			var model = new ResetPasswordViewModel { Code = code, UserId = userId };
			return View(model);
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}
			var user = _userManager.Users.FirstOrDefault(t => t.Id == model.UserId);

			if (user == null)
			{
				// Don't reveal that the user does not exist
				return RedirectToAction(nameof(ResetPasswordConfirmation));
			}
			var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
			if (result.Succeeded)
			{
				return RedirectToAction(nameof(ResetPasswordConfirmation));
			}
			AddErrors(result);
			return View();
		}

		[HttpGet]
		[AllowAnonymous]
		public IActionResult ResetPasswordConfirmation()
		{
			return View();
		}


		[HttpGet]
		public IActionResult AccessDenied()
		{
			return View();
		}

		#region Helpers

		private void AddErrors(IdentityResult result)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
		}

		private IActionResult RedirectToLocal(string returnUrl)
		{
			if (Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}
			else
			{
				return RedirectToAction(nameof(ProductController.Index), "Product");
			}
		}

		#endregion
	}
}
