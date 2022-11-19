namespace ICan.Common.Models.AccountViewModels
{
	public class RoleDescription
	{
		public string Id => NameEn.ToLower();
		public string Name { get; set; }
		public string NameEn { get; set; }
	}
}
