using BenchmarkDotNet.Running;

namespace ICan.BenchmarkTest
{
	public  class Program
	{
		public static void Main(string[] args)
		{
			var summary = BenchmarkRunner.Run<PrintOrderBenchmark>();
		}
	}
}
