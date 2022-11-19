using AutoMapper;
using ICan.Common.Domain;
using ICan.Common.Models.Opt;
using ICan.Data.Context;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class CounterPartyManager : BaseManager
	{
		public CounterPartyManager(IMapper mapper, ApplicationDbContext context, ILogger<BaseManager> logger) : base(mapper, context, logger)
		{
		}

		public IEnumerable<CounterpartyModel> Get()
		{
			var list = _context.OptCounterparty.Include(party => party.PaperOrderRole);
			var modelList = _mapper.Map<IEnumerable<CounterpartyModel>>(list);
			return modelList;
		}

		public async Task<CounterpartyModel> Get(long? id)
		{
			var raw = await _context.OptCounterparty.FirstOrDefaultAsync(m => m.CounterpartyId == id);
			var model = _mapper.Map<CounterpartyModel>(raw);
			return model;
		}

		public async Task Add(CounterpartyModel model)
		{
			var counterparty = new OptCounterparty
			{
				Name = model.Name,
				Inn = model.Inn,
				PaperOrderRoleId = model.PaperOrderRoleId,
				Enabled = model.Enabled,
				Consignee = model.Consignee,
				PaymentDelay = model.PaymentDelay
			};

			_context.Add(counterparty);
			await _context.SaveChangesAsync();
		}

		public async Task Update(int id, CounterpartyModel model)
		{
			var raw =
				_context.OptCounterparty.Find(id);
			raw.Name = model.Name;
			raw.PaperOrderRoleId = model.PaperOrderRoleId;
			raw.Enabled = model.Enabled;
			raw.Inn = model.Inn;
			raw.Consignee = model.Consignee;
			raw.PaymentDelay = model.PaymentDelay;

			_context.Update(raw);
			await _context.SaveChangesAsync();
		}

		public async Task Delete(int id)
		{
			var raw = await _context.OptCounterparty.FirstOrDefaultAsync(m => m.CounterpartyId == id);
			_context.Remove(raw);
			await _context.SaveChangesAsync();
		}

		public IEnumerable<SelectListItem> GetPaperOrderRoles()
		{
			return _context
					.OptPaperOrderRole
					.Select(oRole => new SelectListItem { Text = oRole.Name, Value = oRole.PaperOrderRoleId.ToString() })
					.ToList();
		}

	}
}
