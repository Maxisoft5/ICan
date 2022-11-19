using ICan.Data.Context;

namespace ICan.Data.Repositories
{
	public class BaseRepository
	{
		protected ApplicationDbContext _context { get; }
		
		public BaseRepository(ApplicationDbContext context)
		{
			_context = context;
		}
	}
}