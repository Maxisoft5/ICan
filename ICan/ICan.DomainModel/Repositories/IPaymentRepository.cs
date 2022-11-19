using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface IPaymentRepository
	{
		Task<int> Add(OptPayment payment);
		Task Delete(int id);
		Task<IEnumerable<OptPayment>> GetByOrdersId(int id, PaymentType paymentType);
	}
}
