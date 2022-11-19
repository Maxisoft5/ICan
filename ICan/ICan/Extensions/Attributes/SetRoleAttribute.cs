using ICan.Common;
using ICan.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Extensions
{
	public class SetRoleAttribute : IAsyncResultFilter
	{
		private readonly UserManager<ApplicationUser> _userManager;
		public SetRoleAttribute(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}

		public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
		{
			await SetAdmin((Controller)context.Controller, context.HttpContext);
			await next();
		}

		private async Task SetAdmin(Controller controller, HttpContext context)
		{
			if (context.User.Identity.IsAuthenticated)
			{
				var user = await _userManager.GetUserAsync(context.User);
				var userRoles = await _userManager.GetRolesAsync(user);

				foreach (var role in Const.Roles.RoleDescriptionList)
				{
					controller.ViewData[$"Is{role.NameEn}"] = userRoles.Contains(role.NameEn) ? true : false;
				}

				controller.ViewData["IsClient"] = userRoles == null || !userRoles.Any();
				controller.ViewData["IsMobileBrowser"] = context.Request.IsMobileBrowser();
			}
		}
	}
}
