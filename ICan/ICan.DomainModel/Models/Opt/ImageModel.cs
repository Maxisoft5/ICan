using ICan.Common.Models.Enums;
using ICan.Common.Utils;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class ImageModel
	{
		public int ImageId { get; set; }

		public ImageObjectType ObjectTypeId { get; set; }

		[Display(Name = "Порядок отображения")]
		public int Order { get; set; }

		[Display(Name = "Файл иллюстрации")]
		public string FileName { get; set; }

		[Display(Name = "Файл иллюстрации")]
		public string UserFileName { get; set; }

		[Display(Name = "Тип иллюстрации")]
		public ProductImageType ImageType { get; set; }

		[Display(Name = "Тип иллюстрации")]
		public string DisplayImageType => ImageType.GetDisplayName();

		public int ObjectId { get; set; }
		public string FileFullPath => !string.IsNullOrWhiteSpace(FileName)
		? $"{BucketUrl}/{FileName}"
		: string.Empty;

		public string BucketUrl { get; set; }
		public string ImageUrl { get; set; }
		public string ContentType => Util.GetContentType(FileName);
	}
}
