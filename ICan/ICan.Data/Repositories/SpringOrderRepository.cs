using ICan.Common.Domain;
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
	public class SpringOrderRepository : BaseRepository, ISpringOrderRepository
	{
		public SpringOrderRepository(ApplicationDbContext context) : base(context)
		{
		}

		public async Task<int> Add(OptSpringOrder springOrder)
		{
			await _context.AddAsync(springOrder);
			await _context.SaveChangesAsync();
			return springOrder.SpringOrderId;
		}

		public async Task Delete(int id)
		{
			var springOrder = await _context.OptSpringOrder.FirstOrDefaultAsync(x => x.SpringOrderId == id);
			if (springOrder == null)
				throw new UserException("Указанный заказ пружины не найден");

			_context.Remove(springOrder);
			await _context.SaveChangesAsync();
		}

		public async Task<OptSpringOrder> GetById(int id)
		{
			return await _context.OptSpringOrder
				.Include(springOrder => springOrder.SpringOrderIncomings)
				.Include(springOrder => springOrder.Spring)
				.ThenInclude(spring => spring.NumberOfTurns)
				.FirstOrDefaultAsync(x => x.SpringOrderId == id);
		}

		public IQueryable<OptSpringOrder> GetSpringOrders()
		{
			return   _context.OptSpringOrder
				.Include(sOrder => sOrder.Spring);
		}

		public async Task Update(OptSpringOrder sprintOrder)
		{
			_context.OptSpringOrder.Update(sprintOrder);
			await _context.SaveChangesAsync();
		}
	}
}
