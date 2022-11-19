using System.Collections.Generic;

namespace ICan.Common.Models.Mailchimp
{
	public class ActualLists
	{
		public List<ActualList> Lists { get; set; }
	}


	public class ActualList
	{
		public string Id { get; set; }
	}
}
