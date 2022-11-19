using AutoMapper;
using ICan.Business.Managers;
using ICan.Common.Models;
using ICan.Common.Utils;
using ICan.Data.Context;
using ICan.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICan.Controllers
{

	public class SettingsController : Controller
	{
		protected readonly ILogger<SettingsController> _logger;
		protected IConfiguration _configuration { get; }

		public SettingsController(
				ILogger<SettingsController> logger,
				IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration;
		}

		[Authorize]
		public string Get(string settingName)
		{
			return _configuration[settingName];
		}

		[AllowAnonymous]
		public string GetAnonymous(string settingName)
		{
			return _configuration[settingName];
		}
	}
}