using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace ICan.Jobs.Runner.Extensions
{
	public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
	{
		public bool Authorize([NotNull] DashboardContext context)
		{
			return context.GetHttpContext().User.Identity.IsAuthenticated;
		}
	}
}
