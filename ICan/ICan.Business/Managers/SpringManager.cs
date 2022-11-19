using AutoMapper;
using ICan.Common.Domain;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class SpringManager : BaseManager
	{
		private readonly ISpringRepository _springRepository;
		private readonly INumberOfTurnsRepository _numberOfTurnsRepository;

		public SpringManager(IMapper mapper, ApplicationDbContext context, ILogger<BaseManager> logger,
			ISpringRepository springRepository, INumberOfTurnsRepository numberOfTurnsRepository) : base(mapper, context, logger)
		{
			_springRepository = springRepository;
			_numberOfTurnsRepository = numberOfTurnsRepository;
		}

		public async Task<IEnumerable<SpringModel>> GetSprings()
		{
			var springs = await _springRepository.Get();
			return _mapper.Map<IEnumerable<SpringModel>>(springs);
		}

		public async Task Create(SpringModel model)
		{
			var optSpring = new OptSpring
			{
				BlockThickness = model.BlockThickness,
				Step = model.Step,
				SpringName = model.SpringName,
				NumberOfTurnsId = model.NumberOfTurnsId
			};
			await _springRepository.Create(optSpring);
		}

		public async Task<SpringModel> GetById(int id)
		{
			var spring = await _springRepository.GetById(id);
			return _mapper.Map<SpringModel>(spring);
		}

		public async Task Edit(SpringModel model)
		{
			var mappedModel = _mapper.Map<OptSpring>(model);
			await _springRepository.Update(mappedModel);
		}

		public async Task Delete(int id)
		{
			await _springRepository.Delete(id);
		}

		public async Task<IEnumerable<OptNumberOfTurns>> GetNumberOfTurns()
		{
			return await _numberOfTurnsRepository.Get();
		}
	}
}
