using ICan.Common.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface IDiscountRepository
	{
		Task<long> Add(OptDiscount raw);
		Task<OptDiscount> Get(long discountId);
		IQueryable<OptDiscount> Get();
		Task Archive(OptDiscount raw);
	}
}