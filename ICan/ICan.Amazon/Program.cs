using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net;
using System.Web;

namespace ICan.Amazon
{
	class Program
	{
		static string Endpoint = "https://sellingpartnerapi-eu.amazon.com";
		static string MarketpaceId = "A1F83G8C2ARO7P";
		static void Main(string[] args)
		{
			String accessToken = TokenHandler.GetAccessToken();
			var client = new RestClient(Endpoint);
			//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
			ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

			RestRequest request = new RestRequest($"/catalog/v0/items/0321200683?MarketplaceId={MarketpaceId}");//&ISBN=978-0321200686
			//request.AddHeader("host", "eu-west-1");			
			request.AddHeader("host", "sellingpartnerapi-eu.amazon.com");			
			request.AddHeader("x-amz-access-token", accessToken);
			request.AddHeader("x-amz-date", DateTime.Now.ToUniversalTime()
						 .ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"));
			//request.AddHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.128 Safari/537.36");
			IRestResponse response = client.Post(request);


			String json = response.Content;
			Console.WriteLine(accessToken);
		}

		public class TokenRequest
		{
			public String refresh_token;
			public String client_id;
			public String client_secret;
			public String grant_type = "refresh_token";
		}

		public class TokenResponse
		{
			public String access_token;
			public String token_type;       // \":\"bearer\",
			public int expires_in;      // \":3600}"
		}



		public class TokenHandler
		{
			private static DateTime expDateTime = DateTime.Now;
			private static TokenResponse accessToken;

			public static string GetAccessToken()
			{
				//Validate the return anti-forgery token against the user's session to prevent confused deputy security exploits such as CSRF and XSS

				DateTime now = DateTime.Now;

				// Access Tokens are valid for one hour. Re-use token if not expired

				if (accessToken == null || DateTime.Compare(now, expDateTime) > 0)
				{
					TokenRequest tokenRequest = new TokenRequest()
					{
						refresh_token = "Atzr|IwEBIE34p4-LBEQ6DkQsgODoVftIhj3LQoFiMd7nKlp2zzobNyxRK1eEJSSQ3TkK2L9a0Sb2L9W8asYukQuVgfSV3VJlef6lyWOwox4wmURE0G_XzU79xzwfbx7nnFowyMxL5PgbvIChsECsuVLIqXjKHGC1zRlF-Khz3vBTgiGwy8gabZpF6rzJWn8Ba8NQmL-PBaCwnlm_jLENm8QED0nR83vdxMzEe54vz_ArVuSjwzqau6QI8f3I1G7allw1g3cneN-_DM-Npn8g20-s7dkdvZ3pDqRFLqjGJlB3MOJ7kwwgCvXcQbUnpEw_wfxF_tu0TV8",//AmazonLWA_RefreshToken,
						client_id = "amzn1.application-oa2-client.84b769ae075e4afe9ff5d05b68ca8815",
						client_secret = "b8c956ab7d85a55fd8d3d98f60d20150a7b398eef8cbdda05974e56a3302aa0c",
					};

					RestClient client = new RestClient("https://api.amazon.com");

					RestRequest request = new RestRequest("/auth/o2/token");

					request.AddJsonBody(tokenRequest);

					IRestResponse response = client.Post(request);


					String json = response.Content;
					if (response.IsSuccessful)
					{
						accessToken = JsonConvert.DeserializeObject<TokenResponse>(json);
						expDateTime = DateTime.Now.AddSeconds(accessToken.expires_in - 10); // Subtract 10s as buffer
					}
					else
					{
						accessToken = new TokenResponse();
						accessToken.access_token = "";
					}
				}

				return accessToken.access_token;
			}
		}
	}
}