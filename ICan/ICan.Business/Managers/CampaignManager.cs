using AutoMapper;
using ICan.Business.Services;
using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using ICan.Common.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
    public class CampaignManager : BaseManager
	{
		private readonly ICampaignRepository _campaignRepository;
		private readonly ImageManager _imageManager;
		private readonly IImageRepository _imageRepository;
		private readonly IUserRepository _userRepository;
		private readonly CloudConfiguration _cloudConfiguration;
		private readonly IEmailService _emailService;


		public CampaignManager(IMapper mapper, ILogger<BaseManager> logger, ICampaignRepository campaignRepository,
			IConfiguration configuration, IImageRepository imageRepository,
			IOptions<CloudConfiguration> cloudConfig, IUserRepository userRepository,
			ImageManager imageManager, IEmailService emailService) : base(mapper, logger, configuration)
		{
			_campaignRepository = campaignRepository;
			_imageRepository = imageRepository;
			_cloudConfiguration = cloudConfig.Value;
			_imageManager = imageManager;
			_userRepository = userRepository;
			_emailService = emailService;
		}

		public async Task AddCampaign(CampaignModel campaign)
		{
			await _campaignRepository.Add(_mapper.Map<OptCampaign>(campaign));
		}

		public IEnumerable<CampaignModel> GetCampaigns()
		{
			var campaigns = _campaignRepository.Get().OrderByDescending(x => x.Date).ToList();
			return _mapper.Map<IEnumerable<CampaignModel>>(campaigns);
		}

		public async Task<CampaignModel> Get(int id)
		{
			var campaign = await _campaignRepository.GetById(id);
			var campaignModel = _mapper.Map<CampaignModel>(campaign);
			var images = await _imageRepository.GetImagesByObjectType(id, (int)ImageObjectType.Campaign);
			campaignModel.Images = _mapper.Map<IEnumerable<ImageModel>>(images);
			campaignModel.Images.ToList().ForEach(x => x.ImageUrl = GetImageURL(x.FileName));
			return campaignModel;
		}

		public async Task<int> ExportContacts()
		{
			var contacts = await _userRepository.GetUsers().Where(x => x.IsClient == true 
						&& (x.ClientType == 1 || x.ClientType == 2))
						.ToListAsync();
			return await _emailService.ExportContacts(contacts);
		}

		public async Task Edit(CampaignModel campaign)
		{
			await _campaignRepository.Update(_mapper.Map<OptCampaign>(campaign));
		}

		public async Task Delete(int id)
		{
			await _campaignRepository.Delete(id);
			var images = await _imageRepository.GetImagesByObjectType(id, (int)ImageObjectType.Campaign);
			foreach (var image in images)
			{
				await _imageManager.DeleteImage(image.ImageId);
			}
		}

		public async Task SendCampaign(int id)
		{
			var campaign = await _campaignRepository.GetById(id);
			if (string.IsNullOrWhiteSpace(campaign.ExternalCampaignId))
			{
				throw new UserException("Необходимо сначала подготовить кампанию в сервисе");
			}
			await _emailService.SendCampaign(campaign.ExternalCampaignId);
			campaign.IsSent = true;
			await _campaignRepository.Update(campaign);
		}

		public async Task<string> PrepareCampaign(int id)
		{
			var campaign = await _campaignRepository.GetById(id);
			if(campaign == null)
			{
				_logger.LogError("Can't find campaign by {CampaignId}", id);
				throw new UserException("Ошибка при подготовке кампании");
			}
			campaign.ExternalCampaignId = await _emailService.PrepareCampaign(campaign);
			await _campaignRepository.Update(campaign);
			return campaign.ExternalCampaignId;
		}

		public async Task<string> AddImage(IFormFile file, int campaignId)
		{
			var image = new ImageModel
			{
				ObjectId = campaignId,
				ImageType = ProductImageType.TextImage,
				ObjectTypeId = ImageObjectType.Campaign,
				UserFileName = file.FileName
			};
			var key = $"campaign{campaignId}";
			var fileName = await _imageManager.AddImageAsync(image, file, key);
			return GetImageURL(fileName);
		}

		private string GetImageURL(string fileName)
		{
			return $"{_cloudConfiguration.ObjectStorage}{_cloudConfiguration.BucketName}/{fileName}";
		}
	}
}
