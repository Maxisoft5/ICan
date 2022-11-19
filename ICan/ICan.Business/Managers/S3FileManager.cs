using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using ICan.Common.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class S3FileManager : BaseManager
	{
		private CloudConfiguration _cloudConfiguration;

		public S3FileManager(ILogger<S3FileManager> logger,
			IConfiguration configuration, 
			IOptions<CloudConfiguration> cloudConfig) : base(logger, configuration)
		{			
			_cloudConfiguration = cloudConfig.Value;
		}

		public async Task<string> SaveFileAsync(IFormFile file, string key = "")
		{
			if (file.Length > 0)
			{
				AmazonS3Client s3client = GetAmazonClient();

				var fileFormat = file.FileName.Split(".").Last();
				var fileName = Guid.NewGuid().ToString() + "." + fileFormat;
				if (!string.IsNullOrWhiteSpace(key)) 
				{
					PutObjectRequest request = new PutObjectRequest()
					{
						BucketName = _cloudConfiguration.BucketName,
						Key = key // <-- in S3 key represents a path  
					};

					PutObjectResponse response = await s3client.PutObjectAsync(request);
					fileName = $"{key}/{file.FileName}";
				}

				using (var ms = new MemoryStream())
				{
					file.CopyTo(ms);

					
					var fileTransferUtility =
							new TransferUtility(s3client);

					await fileTransferUtility.UploadAsync(ms,
											   _cloudConfiguration.BucketName, fileName);

					return fileName;
				}
			}

			return string.Empty;
		}

		public async Task RemoveOldFileAsync(string fileName)
		{
			var s3Client = GetAmazonClient();
			var deleteObjectRequest = new DeleteObjectRequest
			{
				BucketName = _cloudConfiguration.BucketName,
				Key = fileName
			};
			await s3Client.DeleteObjectAsync(deleteObjectRequest);
		}
		
		public async Task<byte[]> GetFile(string fileName)
		{
			var s3Client = GetAmazonClient();
			GetObjectRequest request = new GetObjectRequest
			{
				BucketName = _cloudConfiguration.BucketName,
				Key = fileName
			};
			// Issue request and remember to dispose of the response
			using (GetObjectResponse response = await s3Client.GetObjectAsync(request))
			{
				using (MemoryStream ms = new MemoryStream())
				{
					response.ResponseStream.CopyTo(ms);
					return ms.ToArray();
				}				 
			}
			
		}

		public string GetBucketUrl()
		{
			return _cloudConfiguration.BucketUrl;
		}

		private AmazonS3Client GetAmazonClient()
		{
			AmazonS3Config configsS3 = new AmazonS3Config
			{
				ServiceURL = _cloudConfiguration.ObjectStorage,
			};

			AmazonS3Client s3client = new AmazonS3Client(
				_cloudConfiguration.AccessKey,
				_cloudConfiguration.SecretAccessKey,
				configsS3
			);
			return s3client;
		}
	}
}
