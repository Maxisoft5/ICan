using ICan.DomainModel.Messages;
using ICan.Jobs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ICan.Business.Services
{
	public class SmsSenderJob
	{
		private static readonly HttpClient _client = new HttpClient();
		private readonly ILogger<SmsSenderJob> _logger;
		private readonly string _smsLogin;
		private readonly string _smsPassword;
		private readonly string _smsApiUrl;
		private readonly string _callApiUrl;
		private readonly string _queueName;
		private readonly IModel _rbmqModel;

		public SmsSenderJob(IConfiguration configuration,
			IModel rbmqModel,
			ILogger<SmsSenderJob> logger)
		{
			_logger = logger;
			_smsLogin = configuration["Settings:SMS:UserLogin"];
			_smsPassword = configuration["Settings:SMS:UserPassword"];
			_smsApiUrl = configuration["Settings:SMS:ApiUrl"];
			_callApiUrl = configuration["Settings:SMS:ApiCallUrl"];
			_queueName = configuration["Settings:Jobs:WbCheckWarehouse:Queue"];
			_rbmqModel = rbmqModel;
		}

		public void Run()
		{
			var consumer = new EventingBasicConsumer(_rbmqModel);
			consumer.Received += (model, ea) =>
			{
				var body = ea.Body.ToArray();
				var json = Encoding.UTF8.GetString(body);
				SendMessage(JsonConvert.DeserializeObject<SmsMessage>(json));
			};
			_rbmqModel.BasicConsume(queue: _queueName,
									 autoAck: true,
									 consumer: consumer);
		}

		private void SendMessage(SmsMessage message)
		{
			if (message == null || 
				string.IsNullOrWhiteSpace(_smsApiUrl) ||
				string.IsNullOrWhiteSpace(message.Text) ||
				string.IsNullOrWhiteSpace(message.PhoneNumbers))
			{
				_logger.LogWarning($"[SendSMS] #got not sent {message.Text}  {message.PhoneNumbers}");
				return;
			}

			var chunkSize = 1000;
			var smsParts = message.Text.SplitByLength(chunkSize);
			foreach (var smsText in smsParts)
			{
				try
				{
					var url = string.Format(_smsApiUrl, _smsLogin, _smsPassword, message.PhoneNumbers, smsText);
					var callurl = string.Format(_callApiUrl, _smsLogin, _smsPassword, message.PhoneNumbers, smsText);
				 	var resp = _client.GetAsync(url).Result;
				 	_logger.LogWarning($"[SendSMS] #sent {resp.StatusCode} ${resp.ReasonPhrase} ${smsText}");	
					
					resp = _client.GetAsync(callurl).Result;
				 	_logger.LogWarning($"[SendSMS] #called {resp.StatusCode} ${resp.ReasonPhrase} ${smsText}");
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"{ex}  ${smsText}");
				}
			}
		}
	}
}
