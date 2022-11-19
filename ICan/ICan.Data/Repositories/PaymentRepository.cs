using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Repositories;
using ICan.Data.Context;
using ICan.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Data.Repostories
{
	public class PaymentRepository : BaseRepository, IPaymentRepository
	{
		public PaymentRepository(ApplicationDbContext context): base(context)
		{

		}

		public async Task<int> Add(OptPayment payment)
		{
			await _context.AddAsync(payment);
			await _context.SaveChangesAsync();
			return payment.PaymentId;
		}

		public async Task Delete(int id)
		{
			var payment = await _context.OptPayment.FirstOrDefaultAsync(x => x.PaymentId == id);

			if (payment == null)
				throw new UserException("Указанный платеж не найден");

			_context.Remove(payment);
			await _context.SaveChangesAsync();
		}

		public async Task<IEnumerable<OptPayment>> GetByOrdersId(int id, PaymentType paymentType)
		{
			return await _context.OptPayment.Where(x => x.OrderId == id && x.PaymentType == paymentType).ToListAsync();
		}
	}
}
