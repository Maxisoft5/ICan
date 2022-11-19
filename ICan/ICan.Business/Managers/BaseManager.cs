using AutoMapper;
using ICan.Data.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ICan.Business.Managers
{
	public abstract class BaseManager
	{
		protected readonly IMapper _mapper;
		protected readonly IConfiguration _configuration;

		protected ApplicationDbContext _context { get; set; }

		protected readonly ILogger<BaseManager> _logger;

		public BaseManager() { } //for mocks

		public BaseManager(IMapper mapper, ApplicationDbContext context, ILogger<BaseManager> logger) : this(mapper, logger)
		{
			_context = context;
		}

		public BaseManager(IMapper mapper, ILogger<BaseManager> logger)
			: this(logger)
		{
			_mapper = mapper;
		}	
		
		public BaseManager(ILogger<BaseManager> logger)
		{
			_logger = logger;
		}

		public BaseManager(IMapper mapper, ApplicationDbContext context, ILogger<BaseManager> logger, IConfiguration configuration) : this(mapper, context, logger)
		{
			_configuration = configuration;
		}		
		
		public BaseManager(IMapper mapper, ILogger<BaseManager> logger, IConfiguration configuration) : this(mapper, logger)
		{
			_configuration = configuration;
		}

		public BaseManager(ILogger<BaseManager> logger, IConfiguration configuration) : this(logger)
		{
			_configuration = configuration;
		}

		protected void AddStopWatchLog(Stopwatch sw, string message)
		{
			if (sw == null) return;
			_logger.LogWarning($"[StopWatch] {sw.Elapsed} {message}");
		}
	}
}
