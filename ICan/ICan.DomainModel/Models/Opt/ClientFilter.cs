using Microsoft.AspNetCore.Mvc;

namespace ICan.Common.Models.Opt
{
	public class ClientFilter
	{ 
		public int? CategoryFilter { get; set; }
		public int? Filter { get; set; }
		public int? Tag { get; set; }
		public string NotebookLang { get; set; }
	}
}