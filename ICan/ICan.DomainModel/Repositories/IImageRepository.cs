using ICan.Common.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface IImageRepository
	{
		Task<OptImage> Get(int imageId);
		Task Add(OptImage raw);
		bool ImageByType(int objectId, int imageType);
		Task<IEnumerable<OptImage>> GetImagesByObjectType(int objectId, int objectType);
		Task Remove(OptImage raw);
	}
}