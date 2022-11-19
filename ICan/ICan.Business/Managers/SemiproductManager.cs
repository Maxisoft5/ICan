using AutoMapper;
using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models.Exceptions;
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
	public class SemiproductManager : BaseManager
	{
		private readonly ProductManager _productManager;
		private readonly ISemiproductRepository _semiproductRepository;

		public SemiproductManager(IMapper mapper, ApplicationDbContext context,
			ISemiproductRepository semiproductRepository,
			ProductManager productManager,
			ILogger<BaseManager> logger) : base(mapper, context, logger)
		{
			_productManager = productManager;
			_semiproductRepository = semiproductRepository;
		}

		public IEnumerable<PaperModel> GetAvailablePapers(IEnumerable<long> existingIds)
		{
			existingIds ??= Enumerable.Empty<long>();
			var rawList = _context.OptPaper.Where(paper => !existingIds.Contains(paper.PaperId)).OrderBy(paper => paper.Name);
			return _mapper.Map<IEnumerable<PaperModel>>(rawList);
		}

		public IEnumerable<SemiproductModel> GetSemiproducts(IEnumerable<int> ids)
		{
			var raw = _context.OptSemiproduct
				.Include(semiproduct => semiproduct.SemiproductPapers)
					.ThenInclude(sPaper => sPaper.Paper)
				.Include(semiproduct => semiproduct.BlockType)
				.Where(x => ids.Contains(x.SemiproductId))
				.ToList();
			var model = _mapper.Map<IEnumerable<SemiproductModel>>(raw);
			return model;
		}

		public async Task Create(SemiproductModel model)
		{
			if (model.IsUniversal)
			{
				var parentProduct = await _context.OptProduct.FirstOrDefaultAsync(x => x.ProductId == model.ProductId);
				if (parentProduct.CountryId.HasValue)
					throw new UserException("Универсальная наклейка может быть только у русской тетради");
			}

			if (model.IsUniversal && model.RelatedProducts == null)
				throw new UserException("У универсальных наклеек должна быть хотя бы одна связанная тетрадь");
			var raw = GetRawSemiproduct(model);
			await _semiproductRepository.Add(raw);
		}

		public async Task Update(SemiproductModel model)
		{
			if (model.IsUniversal && model.RelatedProducts == null)
				throw new UserException("У универсальных наклеек должна быть хотя бы одна связанная тетрадь");

			var raw = await _semiproductRepository.GetAsync(model.SemiproductId);

			raw.Description = model.Description;
			raw.FormatId = model.FormatId;
			raw.ProductId = model.ProductId;
			raw.SemiproductTypeId = model.SemiproductTypeId;
			raw.StripNumber = model.StripNumber;
			raw.Name = model.SemiproductTypeId == (int)SemiProductType.Box ? model.Name : null;
			raw.BlockTypeId = model.SemiproductTypeId == (int)SemiProductType.Block ? model.BlockTypeId : null;
			raw.HaveWDVarnish = model.HaveWDVarnish;
			raw.HaveStochastics = model.HaveStochastics;
			raw.CutLength = model.CutLength;
			raw.IsUniversal = model.IsUniversal;


			var modelPaperIds = model.SemiproductPapers.Select(sPaper => sPaper.PaperId);
			var dbPaperIds = raw.SemiproductPapers.Select(sPaper => sPaper.PaperId);
			foreach (var paperFromDb in raw.SemiproductPapers)
			{
				if (!modelPaperIds.Contains(paperFromDb.PaperId))
					_context.Remove(paperFromDb);
			}
			foreach (var modelPaper in model.SemiproductPapers)
			{
				if (!dbPaperIds.Contains(modelPaper.PaperId))
				{
					var newPaper = new OptSemiproductPaper
					{
						PaperId = modelPaper.PaperId,
						Semiproduct = raw
					};
					await _context.AddAsync(newPaper);
				}
			}

			if (model.RelatedProducts != null && model.RelatedProducts.Any())
			{
				var modelRelatedProductIds = model.RelatedProducts.Select(sPaper => sPaper.ProductId);
				var dbRelatedProductIds = raw.RelatedProducts.Select(sPaper => sPaper.ProductId);
				foreach (var relatedProduct in raw.RelatedProducts)
				{
					if (!modelRelatedProductIds.Contains(relatedProduct.ProductId))
						_context.Remove(relatedProduct);
				}
				foreach (var relatedProduct in model.RelatedProducts)
				{
					if (!dbRelatedProductIds.Contains(relatedProduct.ProductId))
					{
						var optRelatedProduct = _mapper.Map<OptSemiproductProductRelation>(relatedProduct);
						optRelatedProduct.SemiproductId = raw.SemiproductId;
						await _context.AddAsync(optRelatedProduct);
					}
				}
			}
			else
			{
				_context.RemoveRange(raw.RelatedProducts);
			}

			_context.Update(raw);
			await _context.SaveChangesAsync();
		}

		public async Task<IEnumerable<OptSemiproductPaper>> GetSemiproductPapers(IEnumerable<int> semiproductPaperids)
		{
			return await _semiproductRepository.GetSemiproductPapers().Where(x => semiproductPaperids.Contains(x.SemiproductPaperId)).ToListAsync();
		}

		public bool CheckModel(SemiproductModel model, out string error)
		{
			error = string.Empty;
			var duplicates = _semiproductRepository.FindDuplicate(model.SemiproductId,
				model.SemiproductTypeId, model.ProductId, model.Name);
			if (duplicates != null && duplicates.Any())
			{
				error = "Невозможно сохранить. Такой полуфабрикат был сохранён ранее";
				return false;
			}

			var availableTypes = GetAvailableSemiproductTypes(model.ProductId)
				.Select(sType => sType.SemiproductTypeId);
			if (!availableTypes.Contains(model.SemiproductTypeId))
			{
				error = "Невозможно сохранить. Тип полуфабриката несовместим с тетрадью";
				return false;
			}
			return true;
		}

		public IEnumerable<SemiproductTypeModel> GetAvailableSemiproductTypes(int current)
		{
			var product = _productManager.GetDetails(current);
			var availableSemiproductTypes = GetAvailableSemiproductTypesByProduct(product);
			var semiproducts = _semiproductRepository.GetSemiproductTypes()
				.Where(semirprod => availableSemiproductTypes.Contains(semirprod.SemiproductTypeId));
			var list = _mapper.Map<IEnumerable<SemiproductTypeModel>>(semiproducts.ToList())
				.OrderBy(semiprodutType => semiprodutType.Name);
			return list;
		}

		public SemiproductModel GetSecondPartOfBox(int semiproductId)
		{
			var boxes = GetSemiproductByTypeList(SemiProductType.Box);
			var firstPart = boxes.First(x => x.SemiproductId == semiproductId);

			var secondPart = boxes.FirstOrDefault(x => x.ProductId == firstPart.ProductId && x.SemiproductId != firstPart.SemiproductId);
			return secondPart;
		}

		public async Task<IEnumerable<SemiproductModel>> GetSemiproductList()
		{
			var raw = await _semiproductRepository.Get()
				.ToListAsync();
			var list = _mapper.Map<IEnumerable<SemiproductModel>>(raw);
			return list;
		}

		public IQueryable<OptSemiproduct> GetSemiproductsRaw()
		{
			var raw = _semiproductRepository.Get();
			return raw;
		}

		public async Task<SemiproductModel> GetSemiproductAsync(int semiproductId)
		{
			var raw = await _semiproductRepository.GetAsync(semiproductId);

			if (raw == null)
				return null;

			var model = _mapper.Map<SemiproductModel>(raw);
			return model;
		}

		public async Task<IEnumerable<OptSemiproduct>> GetSemiproductsWithTypes()
		{
			var rawList = await _semiproductRepository.GetSemiproductsWithTypes()
				.ToListAsync();
			return rawList;
		}

		public IEnumerable<SemiproductTypeModel> GetSemiproductType(bool excludeCursAnPointers = true)
		{
			IQueryable<OptSemiproductType> raw = _semiproductRepository.GetSemiproductTypes();

			if (excludeCursAnPointers)
			{
				raw = raw.Where(sm => sm.SemiproductTypeId != (int)SemiProductType.Cursor
				&& sm.SemiproductTypeId != (int)SemiProductType.Pointer
				);
			};

			var list = _mapper.Map<IEnumerable<SemiproductTypeModel>>(raw.ToList());
			return list;
		}

		public IEnumerable<FormatModel> GetFormat()
		{
			var raw = _semiproductRepository.GetFormat().ToList();
			var list = _mapper.Map<IEnumerable<FormatModel>>(raw);
			return list;
		}

		public IEnumerable<PaperModel> GetPaper()
		{
			var raw = _semiproductRepository.GetPaper().ToList();
			var list = _mapper.Map<IEnumerable<PaperModel>>(raw);
			return list;
		}

		public async Task DeleteAsync(int id)
		{
			var semiProduct = await _context.OptSemiproduct.FirstOrDefaultAsync(semiprod => semiprod.SemiproductId == id);
			if (semiProduct == null)
				return;
			var relatedProducts = _context.OptSemiproductProductRelation.Where(semiprodRel => semiprodRel.SemiproductId == id);
			_context.Remove(semiProduct);
			_context.RemoveRange(relatedProducts);
			await _context.SaveChangesAsync();
		}

		private IEnumerable<int> GetAvailableSemiproductTypesByProduct(ProductModel product)
		{
			var semiproductTypes = new List<int>();
			if (product.IsKit || Const.BoboProductIds.Contains(product.ProductId))
			{
				semiproductTypes.Add((int)SemiProductType.Stickers);
				semiproductTypes.Add((int)SemiProductType.Box);
				semiproductTypes.Add((int)SemiProductType.Cursor);
			}
			else if (product.ProductSeriesId == Const.CalendarSeriedId)
			{
				semiproductTypes.Add((int)SemiProductType.Block);
				semiproductTypes.Add((int)SemiProductType.Stickers);
				semiproductTypes.Add((int)SemiProductType.Cursor);
			}
			else
			{
				semiproductTypes.Add((int)SemiProductType.Block);
				semiproductTypes.Add((int)SemiProductType.Covers);
				semiproductTypes.Add((int)SemiProductType.Stickers);
				semiproductTypes.Add((int)SemiProductType.Cursor);
				semiproductTypes.Add((int)SemiProductType.Pointer);
			}
			return semiproductTypes;
		}

		private IEnumerable<SemiproductModel> GetSemiproductByTypeList(SemiProductType semiProductType)
		{
			var raw = _semiproductRepository.Get().Where(semiproduct => semiproduct.SemiproductTypeId == (int)semiProductType);
			var list = _mapper.Map<IEnumerable<SemiproductModel>>(raw);
			return list;
		}

		private static OptSemiproduct GetRawSemiproduct(SemiproductModel model)
		{
			var raw = new OptSemiproduct
			{
				ProductId = model.ProductId,
				FormatId = model.FormatId,
				SemiproductTypeId = model.SemiproductTypeId,
				StripNumber = model.StripNumber,
				Name = model.SemiproductTypeId == (int)SemiProductType.Box ? model.Name : null,
				BlockTypeId = model.BlockTypeId == (int)SemiProductType.Block ? model.BlockTypeId : null,
				HaveWDVarnish = model.HaveWDVarnish,
				HaveStochastics = model.HaveStochastics,
				CutLength = model.CutLength,
				IsUniversal = model.IsUniversal,
				SemiproductPapers = model.SemiproductPapers.Select(paper => new OptSemiproductPaper { PaperId = paper.PaperId }).ToList(),
				RelatedProducts = model.RelatedProducts?.Select(rProduct => new OptSemiproductProductRelation { ProductId = rProduct.ProductId }).ToList(),
			};

			return raw;
		}
	}
}
