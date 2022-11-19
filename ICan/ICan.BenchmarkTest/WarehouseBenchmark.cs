using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using ICan.Data.Context;
using ICan.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace ICan.BenchmarkTest
{
	[SimpleJob(RunStrategy.ColdStart, targetCount: 500000)]
	public class WarehouseBenchmark
	{
		[Benchmark]
		public void GetList()
		{
			var options = new DbContextOptions<ApplicationDbContext>();
			var connectionstring = "server=localhost;database=yamoguopt_db;user=root;password=654321;persistsecurityinfo=True;SslMode=none;Allow User Variables=true;AllowPublicKeyRetrieval=True";

			var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
			optionsBuilder.UseMySql(connectionstring, ServerVersion.AutoDetect(connectionstring));
			using (ApplicationDbContext dbContext = new ApplicationDbContext(optionsBuilder.Options))
			{
				 
			}
		}
	}
}
