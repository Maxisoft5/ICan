using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using AutoMapper;
using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using ICan.Common.Utils;
using ICan.Data.Context;
using Ionic.Zip;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class MaterialManager : BaseManager
	{
		private readonly IMaterialRepository _materialRepository;
		private readonly ImageManager _imageManager;


		public MaterialManager(
			ImageManager imageManager,
			IMaterialRepository materialRepository,
			IMapper mapper, ILogger<ShopManager> logger) : base(mapper, logger)
		{
			_materialRepository = materialRepository;
			_imageManager = imageManager;

		}

		public IEnumerable<MaterialModel> Get(bool onlyActive = true)
		{
			var rawList = _materialRepository.Get(onlyActive);

			var list = _mapper.Map<List<MaterialModel>>(rawList);
			_imageManager.SetImagePath(list);
			return list.OrderByDescending(material => material.Date);
		}


		public async Task<MaterialModel> Get(int id)
		{
			var raw = _materialRepository.Get(false, id).FirstOrDefault();
			var model = _mapper.Map<MaterialModel>(raw);
			_imageManager.SetSingleImagePath(model);
			return model;
		}

		public async Task<MaterialModel> GetByFileName(string fileName)
		{
			var raw = _materialRepository.GetByFileName(fileName);
			var model = _mapper.Map<MaterialModel>(raw);
			_imageManager.SetSingleImagePath(model);
			return model;
		}

		public async Task AddAsync(MaterialModel model)
		{
			var raw = _mapper.Map<OptMaterial>(model);
			await _materialRepository.AddAsync(raw);
			if (model.Images != null && model.Images.Any())
			{
				foreach (var image in model.Images)
				{
					image.ObjectId = raw.MaterialId;
					await _imageManager.AddImageAsync(image, null);
				}
			}
		}

		public async Task AddImageAsync(ImageModel image, IFormFile formFile)
		{
			await _imageManager.AddImageAsync(image, formFile);
		}

		public async Task<byte[]> GetMaterialToDownload(MaterialModel model)
		{
			if (model.DownloadFile == null)
				return new byte[0];
			var bytes = await _imageManager.GetFile(model.DownloadFile);
			return bytes;
		}

		public async Task UpdateAsync(MaterialModel model)
		{
			var raw = _materialRepository.GetWithoutImages(model.MaterialId);
			raw.Date = model.Date;
			raw.IsActive = model.IsActive;
			raw.Theme = model.Theme;
			raw.Content = model.Content;

			await _materialRepository.UpdateAsync(raw);
		}

		public async Task DeleteImage(int imageId)
		{
			await _imageManager.DeleteImage(imageId);
		}

		public async Task DeleteAsync(int id)
		{
			var raw = _materialRepository.Get(false, id).First();
			if (raw.Images != null && raw.Images.Any())
			{
				foreach (var image in raw.Images)
				{
					await _imageManager.DeleteImage(image.ImageId);
				}
			}
			await _materialRepository.Delete(raw.Material);

		}


	}
}
