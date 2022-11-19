using ICan.Common.Domain;
using ICan.Common.Repositories;
using ICan.Data.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Data.Repositories
{
	public class MaterialRepository : BaseRepository, IMaterialRepository
	{
		public MaterialRepository(ApplicationDbContext context) : base(context)
		{
		}

		public async Task AddAsync(OptMaterial raw)
		{
			await _context.AddAsync(raw);
			await _context.SaveChangesAsync();
		}
		
		public async Task UpdateAsync(OptMaterial raw)
		{
			_context.Update(raw);
			await _context.SaveChangesAsync();
		}

		public IEnumerable<MaterialImage> Get(bool onlyActive, int? materialId = null)
		{
			IQueryable<OptMaterial> materials = _context.OptMaterial;
			if (onlyActive)
			{
				materials = materials.Where(mat => mat.IsActive);
			}
			if (materialId.HasValue)
			{
				materials = materials.Where(mat => mat.MaterialId == materialId.Value);
			}
			var joinWithImages = from material in materials
								 join image in _context.OptImage on material.MaterialId equals image.ObjectId into subSet
								 from subImage in subSet.DefaultIfEmpty()
								 select new { Material = material, Image = subImage };

			var result = joinWithImages
				.ToList()
				.GroupBy(im => im.Material)
				.Select(group => new MaterialImage { 
					Material = group.Key, 
					Images = group.Select(groupItem => groupItem.Image).Where(image=>  image != null)
				});
	 
			return result;
		}

	 
		public OptMaterial GetWithoutImages(int materialId)
		{
			var raw = _context.OptMaterial
			   .FirstOrDefault(mat => mat.MaterialId == materialId);
			return raw;
		}

		public async Task Delete(OptMaterial material)
		{
			_context.Remove(material);
			await _context.SaveChangesAsync();
		}

		public MaterialImage GetByFileName(string fileName)
		{
			var raw = Get(false)
				.FirstOrDefault(material => material.Images.Any(img =>
				!string.IsNullOrWhiteSpace(img.FileName) &&
				img.FileName.Equals(fileName)));
			return raw;
		}
	}
}
