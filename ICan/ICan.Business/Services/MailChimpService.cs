using ICan.Common.Domain;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Mailchimp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ICan.Business.Services
{
	public class MailChimpService: IEmailService
	{
		private readonly string MailChimpEndpoint;
		private readonly HttpRequestSenderService _httpRequestSender;
		private readonly Dictionary<string, string> authHeader = new();
		private readonly ILogger<MailChimpService> _logger;
		private readonly IConfiguration _configuration;

		public MailChimpService(IConfiguration configuration, IHttpClientFactory httpClientFactory, HttpRequestSenderService httpSender,
			ILogger<MailChimpService> logger)
		{
			MailChimpEndpoint = configuration["Mailchimp:MailChimpEndpoint"];
			_httpRequestSender = httpSender;
			authHeader.Add("Authorization", $"Basic {configuration["Mailchimp:Token"]}");
			_logger = logger;
			_configuration = configuration;
		}

		public async Task<int> ExportContacts(IEnumerable<ApplicationUser> contacts)
		{
			var listId = await GetListId();

			var addCustomFields = new AddCustomField
			{
				ListId = listId,
				Name = "ClientType",
				Type = "number",
				Tag = "ClientType"
			};

			var response = await _httpRequestSender.SendRequest(addCustomFields, MailChimpEndpoint + $"lists/{listId}/merge-fields", RequestType.POST, authHeader);
			//if (!response.IsSuccessStatusCode)
			//	throw new UserException("Не удалось добавить поле \"Тип клиента\"");

			var contactsInfo = contacts.Select(x => GetMemberInfo(x)).ToList();

			var batchMembers = new
			{
				list_id = listId,
				members = contactsInfo
			};

			response = await _httpRequestSender.SendRequest(batchMembers, MailChimpEndpoint + $"/lists/{listId}", RequestType.POST, authHeader);

			if (!response.IsSuccessStatusCode)
			{
				var message = "Не удалось экспортировать контакты";
				_logger.LogWarning($"{message} {JsonConvert.SerializeObject(response, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })}");
				throw new UserException(message);
			}
			await CreateSegment(ClientType.JointPurchase.ToString(), ((int)ClientType.JointPurchase).ToString(), listId);
			await CreateSegment(ClientType.Shop.ToString(), ((int)ClientType.Shop).ToString(), listId);

			return contactsInfo.Count;
		}

		public async Task SendCampaign(string campaignId)
		{
			var response = await _httpRequestSender.SendRequest("", MailChimpEndpoint + $"/campaigns/{campaignId}/actions/send", RequestType.POST, authHeader);

			if (!response.IsSuccessStatusCode)
				throw new UserException("Не удалось отправить рассылку");
		}

		public async Task<string> Createcampaign(CreateCampaignModel campaign)
		{
			var response = await _httpRequestSender.SendRequest(campaign, MailChimpEndpoint + $"campaigns", RequestType.POST, authHeader);

			if (response.IsSuccessStatusCode)
			{
				var responseStr = await response.Content.ReadAsStringAsync();
				var createdcampaign = JsonConvert.DeserializeObject<CreateCampaignResponse>(responseStr);
				if (createdcampaign != null)
					return createdcampaign.Id;
			}

			throw new UserException("Не удалось создать кампанию в сервисе рассылки");
		}

		public async Task CreateSegment(string segmentName, string fieldValue, string listId)
		{
			var shopSegment = new CreateSegmentModel
			{
				List_Id = listId,
				Name = segmentName,
				Options = new SegmentOptions
				{
					Conditions = new List<SegmentCondition>
					{
						{
							new SegmentCondition
							{
								ConditionType = "TextMerge",
								Field = "CLIENTTYPE",
								Option = "is",
								Value = fieldValue
							}
						}
					}
				}
			};

			var response = await _httpRequestSender.SendRequest(shopSegment, MailChimpEndpoint + $"lists/{listId}/segments", RequestType.POST, authHeader);
			if (!response.IsSuccessStatusCode)
			{

				var message = "Не удалось создать сегменты";
				_logger.LogWarning($"{message} {JsonConvert.SerializeObject(response, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })}");
				throw new UserException(message);
			}
		}

		public async Task AddClient(ApplicationUser user)
		{
			var listId = await GetListId();
			var member = GetMemberInfo(user);
			var response = await _httpRequestSender.SendRequest(member, MailChimpEndpoint + $"/lists/{listId}/members", RequestType.POST, authHeader);

			if (!response.IsSuccessStatusCode)
				_logger.LogWarning(await response.Content.ReadAsStringAsync());
		}

		public async Task RemoveClient(ApplicationUser user)
		{
			var listId = await GetListId();
			var response = await _httpRequestSender.SendRequest("", MailChimpEndpoint + $"lists/{listId}/members/{user.Email}/actions/delete-permanent",
				RequestType.POST, authHeader);

			if (!response.IsSuccessStatusCode)
				_logger.LogWarning(await response.Content.ReadAsStringAsync());
		}

		public async Task UpdateClient(ApplicationUser user)
		{
			var member = GetMemberInfo(user);
			var listId = await GetListId();
			await _httpRequestSender.SendRequest(member, MailChimpEndpoint + $"lists/{listId}/members/{user.Email}", RequestType.PUT, authHeader);

			var tags = new List<Tag>
			{
				new Tag
				{
					TagName = GetTagName(user.ClientType == (int)ClientType.Shop ? (int)ClientType.JointPurchase : (int)ClientType.Shop),
					Status = "inactive"
				},
				new Tag
				{
					TagName = GetTagName(user.ClientType),
					Status = "active"
				}
			};

			var updateTagsModel = new UpdateTagsModel
			{
				Tags = tags
			};

			await _httpRequestSender.SendRequest(updateTagsModel, MailChimpEndpoint + $"lists/{listId}/members/{user.Email}/tags", RequestType.POST, authHeader);
		}

		public async Task<string> PrepareCampaign(OptCampaign campaign)
		{
			var segments = await GetSegments();
			var createModel = new CreateCampaignModel
			{
				Recipients = new Recipients
				{
					ListId = segments.SegmentsList.FirstOrDefault().ListId,
					SegmentOptions = new SegmentOptionsCampaign
					{
						SegmentId = GetCampaignType(campaign, segments)
					}
				},
				Settings = new CampaignSettings
				{
					EmailForReply = _configuration["Mailchimp:EmailForReply"],
					FromName = _configuration["Mailchimp:UserName"],
					Subject = campaign.Title,
					Title = campaign.Title
				}
			};

			var campaignId = await Createcampaign(createModel);
			var addContentModel = new AddContentToCampaignModel
			{
				CampaignId = campaignId,
				Html = campaign.Text
			};
			await AddContentToCampaign(campaignId, addContentModel);
			return campaignId;
		}

		private static int GetCampaignType(OptCampaign campaign, Segments segments)
		{

			if (campaign.CampaignType == (int)CampaignType.Shop)
				return segments.SegmentsList.FirstOrDefault(x => x.Name.Equals(CampaignType.Shop.ToString())).SegmentId;
			if (campaign.CampaignType == (int)ClientType.JointPurchase)
				return segments.SegmentsList.FirstOrDefault(x => x.Name.Equals(CampaignType.JointPurchase.ToString())).SegmentId;
			return segments.SegmentsList.FirstOrDefault(x => x.Name.Equals(CampaignType.Test.ToString())).SegmentId;
		}

		private static MailchimpMemberModel GetMemberInfo(ApplicationUser user)
		{
			return new MailchimpMemberModel
			{
				Email = user.Email,
				Tags = new string[] { GetTagName(user.ClientType) },
				MergeFields = new MergeFields
				{
					Name = user.FirstName,
					ClientType = user.ClientType
				}
			};
		}

		private static string GetTagName(int clientType)
		{
			if (clientType == (int)ClientType.Shop)
				return ClientType.Shop.ToString() + "s";

			if (clientType == (int)ClientType.JointPurchase)
				return ClientType.JointPurchase.ToString() + "s";

			return null;
		}

		private async Task<Segments> GetSegments()
		{
			var listId = await GetListId();
			var response = await _httpRequestSender.SendGet(MailChimpEndpoint + $"/lists/{listId}/segments", authHeader);
			if (response.IsSuccessStatusCode)
			{
				var responseAsStr = await response.Content.ReadAsStringAsync();
				return JsonConvert.DeserializeObject<Segments>(responseAsStr);
			}

			throw new UserException("Не удалось получить ни одного сегмента контактов");
		}

		private async Task AddContentToCampaign(string campaignId, AddContentToCampaignModel addContentModel)
		{
			var response = await _httpRequestSender.SendRequest(addContentModel, MailChimpEndpoint + $"campaigns/{campaignId}/content", RequestType.PUT, authHeader);

			if (!response.IsSuccessStatusCode)
				throw new UserException("Не удалось добавить текст кампании");
		}

		private async Task<string> GetListId()
		{
			var listsResponse = await _httpRequestSender.SendGet(MailChimpEndpoint + "/lists", authHeader);
			if (listsResponse.IsSuccessStatusCode)
			{
				var listsString = await listsResponse.Content.ReadAsStringAsync();
				var lists = JsonConvert.DeserializeObject<ActualLists>(listsString);
				if (lists.Lists.Any())
					return lists.Lists.FirstOrDefault().Id;
				else
					throw new UserException("Необходимо создать пустой лист контактов");
			}
			else
			{
				throw new UserException("Не удалось импортировать контакты");
			}
		}
	}
}
