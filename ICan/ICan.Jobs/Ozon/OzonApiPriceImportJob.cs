using ICan.Business.Managers;
using ICan.Common.Jobs.OzonPrice;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ICan.Jobs.Ozon
{
	public class OzonApiPriceImportJob
	{
		private static readonly HttpClient HttpClient = new HttpClient();
		private readonly ILogger<OzonApiPriceImportJob> _logger;
		private readonly IConfiguration _configuration;
		private readonly OzonApiManager _ozonApiManager;

		public OzonApiPriceImportJob(ILogger<OzonApiPriceImportJob> logger, IConfiguration configuration, OzonApiManager ozonApiManager)
		{
			_logger = logger;
			_configuration = configuration;
			_ozonApiManager = ozonApiManager;
		}

		public async Task<bool> Import()
		{
			try
			{
				_logger.LogWarning($"[Job][OzonApiPriceImportJob] started");

				var request = GetRequest();
				var result = await HttpClient.SendAsync(request);
				result.EnsureSuccessStatusCode();
				var body = await result.Content.ReadAsStringAsync();
				var rootResult = JsonConvert.DeserializeObject<Root>(body);
				if (rootResult?.Result?.Items != null && rootResult.Result.Items.Any())
				{
					await _ozonApiManager.UploadRangeAsync(rootResult.Result.Items);
					_logger.LogWarning($"[Job][OzonApiPriceImportJob] finished");
					return true;
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning($"[Job][OzonApiPriceImportJob] exception");
				_logger.LogError(ex, "[Jobs][OzonPrice]");
			}
			return false;
		}

		private HttpRequestMessage GetRequest()
		{
			var url = _configuration["Settings:Jobs:OzonAPIGetPrices:ApiUrl"];
			var clientId = _configuration["Settings:Jobs:OzonAPIGetPrices:ClientId"];
			var apiKey = _configuration["Settings:Jobs:OzonAPIGetPrices:ApiKey"];
			var request = new HttpRequestMessage { RequestUri = new Uri(url), Method = HttpMethod.Post };
			request.Headers.Add("Client-Id", clientId);
			request.Headers.Add("Api-Key", apiKey);
			request.Content = new StringContent("{\"page\": 0,  \"page_size\": 300}");
			return request;
		}
	}
}
