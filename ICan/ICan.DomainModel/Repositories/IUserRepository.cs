using ICan.Common.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface IUserRepository
	{
		IQueryable<ApplicationUser> GetUsers();

		Task<ApplicationUser> GetUserById(string userId);
	}
}
