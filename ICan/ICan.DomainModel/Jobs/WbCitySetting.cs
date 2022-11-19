using System.Collections.Generic;

namespace ICan.DomainModel.Jobs
{
	public class WbCitySetting
	{
		public string Name { get;set;}
		
		public int DaysGap { get;set;}

		public bool Enabled { get;set;}

		public bool ConsiderMono { get;set;}

		public bool ConsiderMonoPallet { get;set;}

		public IEnumerable<string> Dates{ get;set;}
	}
}
