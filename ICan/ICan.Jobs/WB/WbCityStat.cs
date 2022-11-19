namespace ICan.Jobs.WB
{
	public class WbCityStat
	{
		public string Name { get; set; }
		public int Days { get; set; }
		public int? MixLimit { get; set; }
		public int? MonoLimit { get; set; }
		public int? SupersafeLimit { get; set; }
		public int? MonopaletteLimit { get; set; }
		public bool ConsiderMono { get; set; }
		public bool ConsiderMonoPallet { get; set; }
	}
}
