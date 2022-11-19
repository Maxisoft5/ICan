using ICan.Business.Managers;
using ICan.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Threading.Tasks;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class AmazonMarketingController : BaseController
	{
		private readonly AdminManager _adminManager;

		public AmazonMarketingController(
			ILogger<BaseController> logger,
			AdminManager adminManager,
			IConfiguration configuration
			)
			: base(null, logger, configuration)
		{
			_adminManager = adminManager;
		}

		public IActionResult Index() => View();


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ParseAmazonFile(IFormFile file)
		{
			var bytes = await _adminManager.ParseAndClean(file);
			return File(bytes, MediaTypeNames.Application.Octet, $"Амазон СтопСлова.xlsx");
		}
	}
}