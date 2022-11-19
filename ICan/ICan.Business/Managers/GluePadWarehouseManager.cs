using AutoMapper;
using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class GluePadWarehouseManager : BaseManager
	{
		private readonly WarehouseJournalManager _whjManager;
		public GluePadWarehouseManager(
			IMapper mapper, ApplicationDbContext context, 
			ILogger<BaseManager> logger, 
			WarehouseJournalManager whjManager)
			: base(mapper, context, logger)
		{
			_whjManager = whjManager;
		}


		public async Task<IEnumerable<GluePadIncomingModel>> GetIncomings()
		{
			var incomings = await _context.OptGluePadIncoming.OrderByDescending(x => x.IncomingDate).ToListAsync();
			return _mapper.Map<IEnumerable<GluePadIncomingModel>>(incomings);
		}

		public async Task<GluePadIncomingModel> Get(int id)
		{
			var incoming = await _context.OptGluePadIncoming.FirstAsync(x => x.Id == id);
			return _mapper.Map<GluePadIncomingModel>(incoming);
		}

		public async Task Create(GluePadIncomingModel model)
		{
			var raw = _mapper.Map<OptGluePadIncoming>(model);
			       await _context.AddAsync(raw);
			await _context.SaveChangesAsync();

			await _whjManager.AddAsync(new WarehouseJournalModel
			{
				ActionDate = DateTime.Now,
				ActionExtendedTypeId = WhJournalActionExtendedType.WarehouseArrival,
				ActionId = raw.Id.ToString(),
				ActionTypeId = WhJournalActionType.Income,
				Amount = raw.Amount,
				ObjectId = Const.GluePadProductId,
				ObjectTypeId = WhJournalObjectType.GluePad,
				WarehouseTypeId = WarehouseType.GluePads
			});

			await _context.SaveChangesAsync();
		}

		public async Task Update(GluePadIncomingModel model)
		{
			var raw = _mapper.Map<OptGluePadIncoming>(model);
			_context.Update(raw);
			await _context.SaveChangesAsync();

			await _whjManager.UpdateJournal(model);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteIncoming(int id)
		{
			var entry = await _context.OptGluePadIncoming.FindAsync(id);
			if (entry == null)
				return;

			_context.Remove(entry);
			await _context.SaveChangesAsync();
			await _whjManager.RemoveByAction(id.ToString(), (int)WhJournalActionExtendedType.WarehouseArrival);
		}
	}
}
