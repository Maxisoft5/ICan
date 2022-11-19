using Xunit;

namespace ICan.Test.ParseReports
{
	[CollectionDefinition("Context collection")]
	public class DbContextFuxtureCollection: ICollectionFixture<DbContextFixture>
	{
	}
}
