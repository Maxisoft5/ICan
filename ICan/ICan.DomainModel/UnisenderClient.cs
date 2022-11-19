using ICan.Common.Domain;
using ICan.Common.Models;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Unisender;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ICan.Common
{
	public class UnisenderClient
	{
		private readonly HttpClient _httpClient;
		private readonly IOptions<UnisenderSettings> _unisenderSettings;
		private readonly ILogger<UnisenderClient> _logger;
		
		public UnisenderClient(IOptions<UnisenderSettings> unisenderSettings, IHttpClientFactory clientFactory,
			ILogger<UnisenderClient> logger)
		{
			_unisenderSettings = unisenderSettings;
			_httpClient = clientFactory.CreateClient("unisender");
			_logger = logger;
		}

		public async Task<UnisenderResultWrapper<T>> CreateList<T>(string listTitle)
		{
			var url = BuildUrl("createList") + $"&title={listTitle}";
			var response = await _httpClient.GetAsync(url);

			if (response.IsSuccessStatusCode)
			{
				var responseAsStr = await response.Content.ReadAsStringAsync();
				var result = JsonConvert.DeserializeObject<UnisenderResultWrapper<T>>(responseAsStr);
				return result;
			}

			throw new UserException("Не удалось создать новый список");
		}

		public async Task<IEnumerable<UnisenderList>> GetLists()
		{
			var url = BuildUrl("getLists");
			var response = await _httpClient.GetAsync(url);

			if (response.IsSuccessStatusCode)
			{
				var responseAsStr = await response.Content.ReadAsStringAsync();
				var result = JsonConvert.DeserializeObject<UnisenderResultWrapper<List<UnisenderList>>>(responseAsStr);
				return result.Result;
			}

			throw new UserException("Не удалось получить список созданных листов");
		}

		public async Task<UnisenderResultWrapper<UnisenderSubscribeResult>> Subscribe(int listId, ApplicationUser[] contacts)
		{
			var url = BuildUrlWithContactData("importContacts", listId, contacts);
			var response = await _httpClient.PostAsync(url, null);
			
			if (response.IsSuccessStatusCode)
			{
				var responseAsStr = await response.Content.ReadAsStringAsync();
				var result = JsonConvert.DeserializeObject<UnisenderResultWrapper<UnisenderSubscribeResult>>(responseAsStr);
				if (!result.Result.Log.Any())
				{
					return result;
				}

				throw new UserException(result.Result.Log.First().Message);
			}

			throw new UserException("Не экспортировать контакты");
		}

		public async Task<UnisenderCreatedCampaignResult> SendCampaign(string externalCampaignId)
		{
			var url = BuildUrl("createCampaign") + $"&message_id={externalCampaignId}";
			var response = await _httpClient.PostAsync(url, null);

			if (response.IsSuccessStatusCode)
			{
				var responseAsStr = await response.Content.ReadAsStringAsync();
				var result = JsonConvert.DeserializeObject<UnisenderResultWrapper<UnisenderCreatedCampaignResult>>(responseAsStr);
				if (result.Result != null)
				{
					if (string.IsNullOrWhiteSpace(result.Result.Error))
					{
						return result.Result;
					}

					_logger.LogError(result.Result.Error);
				}
			}

			throw new UserException("Не удалось отправить рассылку");
		}

		public async Task Exclude(int[] listIds, ApplicationUser client)
		{
			var url = BuildUrl("exclude") + $"&contact_type=email&contact={client.Email}&list_ids={string.Join(",",listIds)}";
			var response = await _httpClient.PostAsync(url, null);

			if (response.IsSuccessStatusCode)
			{
				var responseAsStr = await response.Content.ReadAsStringAsync();
				var result = JsonConvert.DeserializeObject<UnisenderResultWrapper<UnisenderExcludeResult>>(responseAsStr);
				if(result.Result != null)
				{
					_logger.LogError(result.Result.Error);
				}
			}
		}

		public async Task<int> CreateMessage(int listId, OptCampaign campaign)
		{
			var url = BuildUrlWithCampaign(listId, campaign);
			var response = await _httpClient.PostAsync(url, null);
			if (response.IsSuccessStatusCode)
			{
				var responseAsStr = await response.Content.ReadAsStringAsync();
				var result = JsonConvert.DeserializeObject<UnisenderResultWrapper<ResultId>>(responseAsStr);
				if(result.Result != null)
				{
					return result.Result.Id;
				}
			}

			throw new UserException("Не удалось создать кампанию в сервисе рассылок");
		}

		private string BuildUrl(string methodName)
		{
			return $"{methodName}?api_key={_unisenderSettings.Value.ApiKey}";
		}

		private string BuildUrlWithContactData(string methodName, int listId, ApplicationUser[] contacts)
		{
			var url = BuildUrl(methodName);
			var sb = new StringBuilder(url);
			sb.Append("&field_names[0]=email&field_names[1]=email_list_ids");
			var counter = 0;
			foreach(var contact in contacts)
			{
				sb.Append($"&data[{counter}][0]={contact.Email}&data[{counter}][1]={listId}");
				counter++;
			}
			return sb.ToString();
		}

		private string BuildUrlWithCampaign(int listId, OptCampaign campaign)
		{
			var url = BuildUrl("createEmailMessage");
			var sb = new StringBuilder(url);
			sb.Append($"&sender_name={_unisenderSettings.Value.SenderName}");
			sb.Append($"&sender_email={_unisenderSettings.Value.SenderEmail}");
			sb.Append($"&subject={campaign.Title}");
			sb.Append($"&body={campaign.Text}");
			sb.Append($"&list_id={listId}");
			sb.Append($"&lang=ru");
			return sb.ToString();
		}
	}
}
