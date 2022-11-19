namespace ICan.Common.Utils
{
	public class CloudConfiguration
	{
		public string ObjectStorage { get; set; }
		public string BucketName { get; set; }
		public string AccessKey { get; set; }
		public string SecretAccessKey { get; set; }

		public string BucketUrl => $"{ObjectStorage}{BucketName}";
	}
}
