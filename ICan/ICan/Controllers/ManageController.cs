using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ICan.Common.Models.ManageViewModels;
using ICan.Data.Context;
using ICan.Common;
using ICan.Common.Models.AccountViewModels;
using ICan.Common.Models.Opt;
using AutoMapper;
using ICan.Common.Domain;

namespace ICan.Controllers
{
	[Authorize()]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class ManageController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;

		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly ProductManager _productManager;
		private readonly IConfiguration _configuration;

		private readonly IEmailSender _emailSender;
		private readonly ILogger _logger;
		private readonly UrlEncoder _urlEncoder;
		private readonly IEmailService _emailService;

		private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
		private const string RecoveryCodesKey = nameof(RecoveryCodesKey);

		public ManageController(IMapper mapper,
		  UserManager<ApplicationUser> userManager,
		  SignInManager<ApplicationUser> signInManager,
		  IEmailSender emailSender,
		  ILogger<ManageController> logger,
		  UrlEncoder urlEncoder,
		  ProductManager productManager,
		  IConfiguration configuration,
		   ApplicationDbContext context,
		   IEmailService emailService)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_emailSender = emailSender;
			_logger = logger;
			_urlEncoder = urlEncoder;
			_context = context;
			_productManager = productManager;
			_mapper = mapper;
			_configuration = configuration;
			_emailService = emailService;
		}

		[TempData]
		public string StatusMessage { get; set; }

		[HttpGet]
		[Authorize(Roles = "Admin,Operator")]
		public async Task<IActionResult> List()
		{
			var users = await  _userManager
				.Users
					.Include(user => user.ApplicationUserShopRelations)
					.ThenInclude(x => x.Shop)
				.Select(us => new ClientModel
				{
					Username = us.UserName,
					Email = us.Email,
					PhoneNumber = us.PhoneNumber,
					IsEmailConfirmed = us.EmailConfirmed,
					FirstName = us.FirstName,
					LastName = us.LastName ?? string.Empty,
					Id = us.Id,
					IsClient = us.IsClient,
					ShopName = us.ApplicationUserShopRelations.Any() ? string.Join(", ", us.ApplicationUserShopRelations.Select(x => x.Shop.Name)) : string.Empty,
					ClientType = Enum.Parse<ClientType>(us.ClientType.ToString()),
					DateRegistration = us.DateRegistration
				})
				.OrderBy(user => user.LastName)
				.ToListAsync();

			return View(users);
		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> Index(string id = null)
		{
			ApplicationUser user = null;
			var guid = Guid.Empty;
			if (string.IsNullOrWhiteSpace(id) ||
				(Guid.TryParse(id, out guid) && guid == Guid.Empty))
				user = await _userManager.GetUserAsync(User);
			else
				user = _userManager.Users.FirstOrDefault(t => t.Id == id);

			if (user == null)
			{
				_logger.LogWarning($"Unable to load user with ID {id}");
				return NotFound("Пользователь с таким идентификатором не найден");
			}

			if (user.IsClient == false)
			{
				return RedirectToAction(nameof(EditEmployee), user);
			}

			user.ApplicationUserShopRelations = _context.OptApplicationUserShopRelation.Where(x => x.UserId == user.Id).Include(x => x.Shop).ToList();

			ViewData["IsCurrentUser"] = string.IsNullOrWhiteSpace(id) || (id == _userManager.GetUserId(User));
			ViewData["ThisUser"] = user.Id;

			var shopList = GetShopList();
			ViewData["ShopId"] = shopList;
			var model = _mapper.Map<ClientModel>(user);

			return View(model);
		}

		private IEnumerable<SelectListItem> GetShopList()
		{
			var shops = _context.OptShop.Include(sp => sp.ShopNames).ToList();
			var shopList = shops
				.Select(sho =>
				{
					var names = sho.Name + " " + string.Join(",", sho.ShopNames.Select(name => name.Inn));
					return new SelectListItem { Text = names, Value = sho.ShopId.ToString() };
				});
			return shopList;
		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> ClientPriceList(string id = null)
		{
			ApplicationUser user = null;
			var guid = Guid.Empty;
			var currentUser = await _userManager.GetUserAsync(User);

			if (string.IsNullOrWhiteSpace(id) || string.Equals(currentUser.Id, id, StringComparison.OrdinalIgnoreCase))
				user = currentUser;
			else
			if (User.IsInRole(Const.Roles.Admin) || User.IsInRole(Const.Roles.Operator))
			{
				user = _userManager.Users.FirstOrDefault(t => t.Id == id);
			}

			if (user == null)
			{
				_logger.LogError($"Запрошены цены по пользователю, которого нет,  id {id}");
				throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			ViewData["IsCurrentUser"] = string.IsNullOrWhiteSpace(id) || (id == _userManager.GetUserId(User));
			ViewData["ThisUser"] = user.Id;


			ProductListModel model = await _productManager.GetProducts(onlyWithPrices: true, user: user);
			ViewData["ShowPrices"] = model.ProductGroups.Any();
			model.ReadOnly = true;
			return View(model);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteClient(string id)
		{
			var user = _userManager.Users.FirstOrDefault(t => t.Id == id);

			if (user == null)
			{
				throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}
			try
			{
				await _userManager.DeleteAsync(user);
				await _emailService.RemoveClient(user);
				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
			}
			catch (Exception ex)
			{
				var errorMessage = string.Format(Const.ErrorMessages.CantDeleteClient, user?.FullName);
				_logger.LogError(ex, errorMessage);
				TempData["ErrorMessage"] = errorMessage;
			}
			return RedirectToAction(nameof(List));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Index(ClientModel model)
		{
			if (!ModelState.IsValid)
			{
				var shopList = GetShopList();
				ViewData["ShopId"] = shopList;
				return View(model);
			}
			var needUpdateMailchimpUser = false;
			var user = _userManager.Users.FirstOrDefault(t => t.Id == model.Id);
			if (user == null)
			{
				throw new ApplicationException($"Не найдена информация  о пользователе с ID '{_userManager.GetUserId(User)}'.");
			}

			if (user.ClientType != (int)model.ClientType)
				needUpdateMailchimpUser = true;

			var email = user.Email;
			if (model.Email != email)
			{
				var setEmailResult = _userManager.SetEmailAsync(user, model.Email).Result;
				if (!setEmailResult.Succeeded)
				{
					throw new ApplicationException($"Ошибка при сохранении email пользователя '{user.Id}'. Возможно, пользователь с таким email уже зарегистрирван ");
				}
			}
			_userManager.Users.Where(t => t.Id == user.Id);
			user.LastName = model.LastName;
			user.FirstName = model.FirstName;
			user.ClientType = (int)model.ClientType;
			user.SecurityStamp = Guid.NewGuid().ToString();
			await _userManager.UpdateAsync(user);
			var phoneNumber = user.PhoneNumber;
			if (model.PhoneNumber != phoneNumber)
			{
				var setPhoneResult = _userManager.SetPhoneNumberAsync(user, model.PhoneNumber).Result;
				if (!setPhoneResult.Succeeded)
				{
					throw new ApplicationException($"Ошибка при сохранении телефона пользователя с ID '{user.Id}'.");
				}
			}

			if (model.ShopIds != null)
			{
				var shops = _context.OptShop.Where(x => model.ShopIds.Contains(x.ShopId));
				foreach (var shop in shops)
				{
					var userShopRelation = new OptApplicationUserShopRelation { UserId = user.Id, ShopId = shop.ShopId };
					await _context.AddAsync(userShopRelation);
				}
				await _context.SaveChangesAsync();
			}

			if (needUpdateMailchimpUser)
			{
				await _emailService.UpdateClient(user);
			}
			
			StatusMessage = "Информация успешно обновлена";

			if (User.IsInRole(Const.Roles.Admin) || User.IsInRole(Const.Roles.Operator))
				return RedirectToAction(nameof(List));

			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SendVerificationEmail(ClientModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
			var email = user.Email;
			_emailSender.SendEmailConfirmation(email, callbackUrl);

			StatusMessage = "Ссылка для подтверждения выслана. Пожалуйста, проверьте свой почтовый ящик.";
			return RedirectToAction(nameof(Index));
		}

		[HttpGet]
		[Authorize(Roles = "Admin,Director,Operator")]
		public IActionResult ManualRegister()
		{
			return View();
		}


		[HttpPost]
		public async Task<IActionResult> ManualRegister(ManualRegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = new ApplicationUser
				{
					UserName = model.Email,
					Email = model.Email,
					PhoneNumber = model.Phone,
					FirstName = model.FirstName,
					LastName = model.LastName,
					ClientType = (int)model.ClientType,
					IsClient = true,
					DateRegistration = DateTime.Now.Date
				};
				var result = await _userManager.CreateAsync(user, Const.TemporaryPassword);
				if (result.Succeeded)
				{
					_context.SaveChanges();
					_logger.LogInformation($"User manually registered with email {model.Email}");
					await FinishRegistrationAsync(user, model.Email);
					await _emailService.AddClient(user);
					return RedirectToAction(nameof(List));
				}
				AddErrors(result);
			}
			return View(model);
		}

		[HttpGet]
		[Authorize]
		[AjaxOnly]
		public async Task<IActionResult> GetUserInfo()
		{
			var user = await _userManager.GetUserAsync(User);

			return Json(new
			{
				user.Email,
				user.PhoneNumber,
				user.FirstName,
				user.LastName,
				user.Id,
				user.IsClient,
				isAdmin = User.IsInRole(Const.Roles.Admin),
				isOperator = User.IsInRole(Const.Roles.Operator),
				isAssembler = User.IsInRole(Const.Roles.Assembler),
				isStoreKeeper = User.IsInRole(Const.Roles.StoreKeeper),
				isDesigner = User.IsInRole(Const.Roles.Designer),
				isContentMan = User.IsInRole(Const.Roles.ContentMan)
			});
		}

		[HttpPost]
		public async Task<IActionResult> SendManualRegisterEmail(ClientModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(model.Email);
				await FinishRegistrationAsync(user, model.Email);
			}
			return RedirectToAction(nameof(List));
		}

		[HttpGet]
		public async Task<IActionResult> ChangePassword()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}
			var hasPassword = await _userManager.HasPasswordAsync(user);
			if (!hasPassword)
			{
				return RedirectToAction(nameof(SetPassword));
			}
			ViewData["ChangePassword"] = true;
			ViewData["ThisUser"] = user.Id;
			ViewData["IsCurrentUser"] = string.IsNullOrWhiteSpace(user.Id) || (user.Id == _userManager.GetUserId(User));
			var model = new ChangePasswordViewModel { StatusMessage = StatusMessage };
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
			if (!changePasswordResult.Succeeded)
			{
				AddErrors(changePasswordResult);
				return View(model);
			}

			await _signInManager.SignInAsync(user, isPersistent: false);
			_logger.LogInformation("User changed their password successfully.");
			StatusMessage = "Пароль успешно изменён.";
			ViewData["IsCurrentUser"] = string.IsNullOrWhiteSpace(user.Id) || (user.Id == _userManager.GetUserId(User));
			return RedirectToAction(nameof(ChangePassword));
		}

		[HttpGet]
		public async Task<IActionResult> SetPassword()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			var hasPassword = await _userManager.HasPasswordAsync(user);

			if (hasPassword)
			{
				return RedirectToAction(nameof(ChangePassword));
			}

			var model = new SetPasswordViewModel { StatusMessage = StatusMessage };
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
			if (!addPasswordResult.Succeeded)
			{
				AddErrors(addPasswordResult);
				return View(model);
			}

			await _signInManager.SignInAsync(user, isPersistent: false);
			StatusMessage = "Пароль сохранён.";

			return RedirectToAction(nameof(SetPassword));
		}

		[HttpGet]
		[Authorize(Roles = "Admin")]
		public IActionResult CreateEmployee()
		{
			var model = new EmployeeModel
			{
				Roles = Const.Roles.RoleDescriptionList.Where(x => x.NameEn != Const.Roles.Admin).ToList()
			};
			return View(model);
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> CreateEmployee(EmployeeModel model)
		{
			if (ModelState.IsValid)
			{
				var user = new ApplicationUser
				{
					UserName = model.Email,
					Email = model.Email,
					FirstName = model.FirstName,
					LastName = model.LastName,
					IsClient = false,
					DateRegistration = DateTime.Now.Date
				};
				var result = await _userManager.CreateAsync(user, Const.TemporaryPassword);
				if (result.Succeeded)
				{
					await _userManager.AddToRolesAsync(user, model.CheckedRoles);
					await _context.SaveChangesAsync();
					_logger.LogInformation($"User manually registered with email {model.Email}");
					await FinishRegistrationAsync(user, model.Email);
					return RedirectToAction(nameof(List));
				}
				AddErrors(result);
			}
			model.Roles = Const.Roles.RoleDescriptionList.Where(x => x.NameEn != Const.Roles.Admin).ToList();
			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> EditEmployee(string id = null)
		{
			if (string.IsNullOrWhiteSpace(id))
				return BadRequest("Указанный пользователь не найден");

			var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id);
			var roles = await _userManager.GetRolesAsync(user);
			var emoloyee = new EmployeeModel
			{
				Id = user.Id,
				FirstName = user.FirstName,
				LastName = user.LastName,
				Email = user.Email,
				Phone = user.PhoneNumber,
				DateRegistration = user.DateRegistration.Date,
				Roles = Const.Roles.RoleDescriptionList.Where(x => roles.Contains(x.NameEn)).ToList()
			};

			ViewBag.Roles = Const.Roles.RoleDescriptionList.Where(x => x.NameEn != Const.Roles.Admin).ToList();
			ViewData["IsCurrentUser"] = string.IsNullOrWhiteSpace(id) || (id == _userManager.GetUserId(User));
			return View(emoloyee);
		}


		[HttpPost]
		public async Task<IActionResult> EditEmployee(EmployeeModel model)
		{
			var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == model.Id);

			if (user == null)
				return BadRequest("Указанный пользователь не найден");

			user.FirstName = model.FirstName;
			user.LastName = model.LastName;
			user.Email = model.Email;
			user.PhoneNumber = model.Phone;
			model.Roles = Const.Roles.RoleDescriptionList.Where(x => model.CheckedRoles.Contains(x.NameEn)).ToList();

			var userRoles = await _userManager.GetRolesAsync(user);
			if (User.IsInRole(Const.Roles.Admin))
			{
				await _userManager.RemoveFromRolesAsync(user, userRoles.Where(x => x != Const.Roles.Admin));
				await _userManager.AddToRolesAsync(user, model.CheckedRoles);
			}

			await _context.SaveChangesAsync();
			TempData["StatusMessage"] = "Информация о пользователе успешно обновлена";
			ViewData["IsCurrentUser"] = string.IsNullOrWhiteSpace(model.Id) || (model.Id == _userManager.GetUserId(User));
			ViewBag.Roles = Const.Roles.RoleDescriptionList.Where(x => x.NameEn != Const.Roles.Admin).ToList();
			return RedirectToAction(nameof(List));
		}

		#region private
		private async Task FinishRegistrationAsync(ApplicationUser user, string email)
		{
			var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
			_emailSender.SendEmailConfirmation(email, callbackUrl);
			TempData["StatusMessage"] = "Письмо о завершении регистрации отправлено";
		}

		private void AddErrors(IdentityResult result)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
		}
		#endregion
	}
}
