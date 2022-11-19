using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class ImageManager : BaseManager
	{
		private IImageRepository _imageRepository;
		private S3FileManager  _s3FileManager;

		public ImageManager(
			S3FileManager s3FileManager,
			IImageRepository imageRepository,
			ILogger<ImageManager> logger,
			IConfiguration configuration) : base(logger, configuration)
		{
			_imageRepository = imageRepository;
			_s3FileManager = s3FileManager;
		}

		public async Task DeleteImage(int imageId)
		{
			var raw = await _imageRepository.Get(imageId);
			await _s3FileManager.RemoveOldFileAsync(raw.FileName);
			await _imageRepository.Remove(raw);
		}

		public bool ImageByTypeExists(int objectId, ProductImageType imageType)
		{
			var imageExists = _imageRepository.ImageByType(objectId, (int)imageType);
			return imageExists;
		}

		public async Task<string> AddImageAsync(ImageModel model, IFormFile image, string key = "") 
		{
			var fileName = await _s3FileManager.SaveFileAsync(image, key);
			var raw = new OptImage
			{
				UserFileName = image.FileName,
				FileName = fileName,
				ImageTypeId = (int)model.ImageType,
				Order = model.Order,
				ObjectTypeId = (int)model.ObjectTypeId,
				ObjectId = model.ObjectId,
			};
			await _imageRepository.Add(raw);
			return fileName;
		}

		public void SetImagePath(List<MaterialModel> list)
		{
			if (list == null || !list.Any())
				return;
			var bucketUrl = _s3FileManager.GetBucketUrl();

			list.ForEach(material =>
			{
				SetSingleImagePath(material, bucketUrl);
			});
		}

		public void SetSingleImagePath(MaterialModel material, string bucketUrl = null)
		{
			if (string.IsNullOrWhiteSpace(bucketUrl))
			{
				bucketUrl = _s3FileManager.GetBucketUrl();
			}
			if (material.Images != null && material.Images.Any())
			{
				material.Images = material.Images.Select(image =>
				{
					image.BucketUrl = bucketUrl;
					return image;
				});
			};
		}

		public async Task<byte[]> GetFile(ImageModel model)
		{
			var bytes  = await _s3FileManager.GetFile(model.FileName);
			return bytes;
		}
	}
}
