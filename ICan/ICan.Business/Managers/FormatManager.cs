using AutoMapper;
using ICan.Common.Domain;
using ICan.Data.Context;
using Microsoft.Extensions.Logging;

namespace ICan.Business.Managers
{
	public class FormatManager : CommonManager<OptFormat>
	{
		public FormatManager(IMapper mapper, ApplicationDbContext context, ILogger<BaseManager> logger) :
			base(mapper, context, logger)
		{
		}
	}
}
