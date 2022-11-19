using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using System;


namespace ICan.Extensions
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class AjaxOnlyAttribute : ActionMethodSelectorAttribute
	{
		public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
		{
			return routeContext.HttpContext.Request.IsAjaxRequest();
		}
	}
}
