using Headstart.Common;
using Headstart.Models.Misc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using System;
using System.Linq;
using System.Security.Claims;

namespace Headstart.API.Controllers
{
	public class HeadstartController : BaseController
	{
		protected AppSettings Settings;

		public HeadstartController(AppSettings settings)
		{
			Settings = settings;
		}

		public void RequireOneOf(params CustomRole[] customRoles)
		{
			var str = customRoles.Select(r => Enum.GetName(typeof(CustomRole), r));
			var roles = Context.User.AvailableRoles;
			var isAuthenticated = roles.Intersect(str).Count() > 0 || roles.Contains("FullAccess");
			Require.That(isAuthenticated, new ErrorCode("InsufficientRoles", 401, $"You need a custom role. One of {string.Join(", ", customRoles)} required."));
		}
	}
}
