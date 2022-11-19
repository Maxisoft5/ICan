using AutoMapper;
using ICan.Common.Models.Exceptions;
using ICan.Common.Domain;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using ICan.Data.Context;
using ICan.Common.Models.Opt;

namespace ICan.Business.Managers
{
	public class TypeOfPaperManager
	{
		private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;
		public TypeOfPaperManager(ApplicationDbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		public async Task<IEnumerable<TypeOfPaperModel>> GetTypes()
		{
			var typesOfPaper = await _context.OptTypeOfPaper.ToListAsync();
			return _mapper.Map<IEnumerable<TypeOfPaperModel>>(typesOfPaper);
		}

		public async Task Create(TypeOfPaperModel typeOfPaperModel)
		{
			await CheckDuplicate(typeOfPaperModel);			

			await _context.AddAsync(new OptTypeOfPaper 
				{ 
					Name = typeOfPaperModel.Name,
					Density = typeOfPaperModel.Density,
					PaperType = typeOfPaperModel.PaperType
				}
			);
			await _context.SaveChangesAsync();
		}

		public async Task<TypeOfPaperModel> GetTypeById(int id)
		{
			var typeOfPaper = await _context.OptTypeOfPaper.FirstOrDefaultAsync(x => x.TypeOfPaperId == id);

			if (typeOfPaper == null)
				throw new UserException($"Вид бумаги с указанным id={id} не найден");

			return _mapper.Map<TypeOfPaperModel>(typeOfPaper);
		}

		public async Task Update(TypeOfPaperModel typeOfPaperModel)
		{
			await CheckDuplicate(typeOfPaperModel);

			var typeOfPaper = await _context.OptTypeOfPaper.FirstOrDefaultAsync(x => x.TypeOfPaperId == typeOfPaperModel.TypeOfPaperId);
			typeOfPaper.Name = typeOfPaperModel.Name;
			typeOfPaper.Density = typeOfPaperModel.Density;
			typeOfPaper.PaperType = typeOfPaperModel.PaperType;
			await _context.SaveChangesAsync();
		}

		public async Task Delete(int id)
		{
			var typeOfPaper = await _context.OptTypeOfPaper.FirstOrDefaultAsync(x => x.TypeOfPaperId == id);
			if (typeOfPaper == null)
				throw new UserException($"Вид бумаги с указанным id={id} не найден");
			_context.Remove(typeOfPaper);
			await _context.SaveChangesAsync();
		}

		private async Task CheckDuplicate(TypeOfPaperModel typeOfPaperModel)
		{
			var typeOfPaper = await _context.OptTypeOfPaper.FirstOrDefaultAsync(x => x.Name == typeOfPaperModel.Name
				&& x.Density == typeOfPaperModel.Density
				&& x.PaperType == typeOfPaperModel.PaperType);

			if (typeOfPaper != null)
				throw new UserException($"Вид бумаги с именем '{typeOfPaperModel.Name}' уже существует");
		}
	}
}
