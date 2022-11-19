using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public class OptTypeOfPaper
	{
		public int TypeOfPaperId { get; set; }
		public string Name { get; set; }
		public string PaperType { get; set; }
		public int? Density { get; set; }
		public ICollection<OptPaper> Papers { get; set; }
	}
}
