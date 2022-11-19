using AutoMapper;
using ICan.Common.Domain;
using ICan.Common.Models.Opt;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class EventManager : BaseManager
	{
		public EventManager(IMapper mapper, ApplicationDbContext context, ILogger<BaseManager> logger) : base(mapper, context, logger)
		{
		}

		public IEnumerable<OptEvent> CheckEvent(EventModel eventModel)
		{
			return _context.OptEvent.Where(optEvent => 
				!optEvent.IsDeleted 
				&& optEvent.Enabled 
				&& eventModel.StartDate < optEvent.EndDate
				&& eventModel.EndDate > optEvent.StartDate
				&& eventModel.EventId != optEvent.EventId);
		}

		public async Task Add(EventModel eventModel)
		{
			var optEvent = _mapper.Map<OptEvent>(eventModel);
			_context.Add(optEvent);
			await _context.SaveChangesAsync();
		}

		public async Task<IEnumerable<EventModel>> Get()
		{
			var list = await _context.OptEvent.Where(optEvent => !optEvent.IsDeleted).ToListAsync();
			var events = _mapper.Map<IEnumerable<EventModel>>(list);
			return events;
		}

		public async Task<EventModel> Get(int? id)
		{
			var optEvent = await _context.OptEvent.FirstOrDefaultAsync(oEvent => !oEvent.IsDeleted && oEvent.EventId == id);
			var model = _mapper.Map<EventModel>(optEvent);
			return model;
		}

		public async Task Update(EventModel eventModel)
		{
			var optEvent = _mapper.Map<OptEvent>(eventModel);

			_context.Update(optEvent);
			await _context.SaveChangesAsync();
		}

		public async Task MarkIsDeleted(int id)
		{
			var optEvent = await _context.OptEvent.FirstOrDefaultAsync(m => m.EventId == id);
			optEvent.IsDeleted = true;
			await _context.SaveChangesAsync();
		}

		public EventModel GetEventByDate(DateTime? byDate = null)
		{
			byDate = byDate ?? DateTime.Now;
			var optEvent = _context.OptEvent.Where(oEvent => !oEvent.IsDeleted && oEvent.Enabled)
				.FirstOrDefault(oEvent => oEvent.StartDate <= byDate && (
				(oEvent.EndDate.HasValue && oEvent.EndDate >= byDate)
				|| !oEvent.EndDate.HasValue));
			var model = _mapper.Map<EventModel>(optEvent);
			return model;
		}
	}
}
