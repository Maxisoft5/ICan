using ICan.Common.Domain;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Data.Repositories
{
	public class ImageRepository : BaseRepository, IImageRepository
	{
		public ImageRepository(ApplicationDbContext context) : base(context)
		{
		}

		public async Task Add(OptImage raw)
		{
			await _context.AddAsync(raw);
			await _context.SaveChangesAsync();
		}

		public async Task<OptImage> Get(int imageId)
		{
			return await _context.OptImage.FindAsync(imageId);
		}

		public async Task<IEnumerable<OptImage>> GetImagesByObjectType(int objectId, int objectType)
		{
			return await _context.OptImage.Where(x => x.ObjectId == objectId && x.ObjectTypeId == objectType).ToListAsync();
		}

		public bool ImageByType(int objectId, int imageType)
		{
			return _context.OptImage
				   .Any(img => img.ObjectId == objectId
				   && img.ImageTypeId == (int)imageType
				   );
		}

		public async Task Remove(OptImage raw)
		{
			_context.Remove(raw);
			await _context.SaveChangesAsync();
		}
	}
}
