using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ICan.Business.Services
{
	public enum RequestType
	{
		GET,
		POST,
		PUT,
		PATCH,
		DELETE
	}

	public class HttpRequestSenderService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		public HttpRequestSenderService(IHttpClientFactory httpClientFactory)
		{
			_httpClientFactory = httpClientFactory;
		}

		public async Task<HttpResponseMessage> SendRequest<T>(T body, string url, RequestType requestType = RequestType.POST, Dictionary<string, string> headers = null) 
		{
			var bodyAsJson = JsonConvert.SerializeObject(body);
			var stringContent = new StringContent(bodyAsJson, Encoding.UTF8, "application/json");
			
			var httpClient = _httpClientFactory.CreateClient();

			if (headers != null && headers.Any())
			{
				AddHeaders(httpClient, headers);
			}
			HttpResponseMessage response = requestType switch
			{ 
				RequestType.PUT => await httpClient.PutAsync(url, stringContent),
				RequestType.PATCH => await httpClient.PatchAsync(url, stringContent),
				_ => await httpClient.PostAsync(url, stringContent),
			};

			return response;
		}

		public async Task<HttpResponseMessage> SendGet(string url, Dictionary<string, string> headers = null)
		{
			var client = _httpClientFactory.CreateClient();
			if (headers != null && headers.Any())
				AddHeaders(client, headers);

			return await client.GetAsync(url);
		}

		public static void AddHeaders(HttpClient client, Dictionary<string, string> headers)
		{
			foreach (var header in headers)
				client.DefaultRequestHeaders.Add(header.Key, header.Value);
		}
	}
}
