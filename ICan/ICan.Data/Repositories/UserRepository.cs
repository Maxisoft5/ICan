using ICan.Common.Models;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Data.Repositories
{
	public class UserRepository : BaseRepository, IUserRepository
	{
		public UserRepository(ApplicationDbContext context) : base(context) { }

		public async Task<ApplicationUser> GetUserById(string userId)
		{
			return await _context.Users.FirstOrDefaultAsync(x => x.Id.Equals(userId));
		}

		public IQueryable<ApplicationUser> GetUsers()
		{
			return _context.Users.AsQueryable();
		}
	}
}
