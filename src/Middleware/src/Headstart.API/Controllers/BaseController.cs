using Headstart.Common;
using Headstart.Models.Misc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ordercloud.integrations.library;
using System;
using System.Linq;
using System.Security.Claims;

namespace Headstart.API.Controllers
{
	[Produces("application/json")]
	public class BaseController : Controller
	{
		public VerifiedUserContext VerifiedUserContext;
		public AppSettings Settings;

		public BaseController(AppSettings settings)
		{
			Settings = settings;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			VerifiedUserContext = new VerifiedUserContext(User);
			base.OnActionExecuting(context);
		}

		public void RequireOneOf(params CustomRole[] customRoles)
		{
			var str = customRoles.Select(r => Enum.GetName(typeof(CustomRole), r));
			var roles = VerifiedUserContext.Principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
			var isAuthenticated = roles.Intersect(str).Count() > 0 || roles.Contains("FullAccess");
			Require.That(isAuthenticated, new ErrorCode("InsufficientRoles", 401, $"You need a custom role. One of {string.Join(", ", customRoles)} required."));
		}
	}
}
