using ICan.Common.Models.Enums;
using ICan.Common.Utils;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class ProductImageModel
	{
		public int ProductImageId { get; set; }

		public int ProductId { get; set; }
		 
		[Display(Name = "Порядок отображения")]
		public int Order { get; set; }

		[Display(Name ="Файл иллюстрации")]
		public string FileName { get; set; }	
		
		[Display(Name ="Файл иллюстрации")]
		public string UserFileName { get; set; }

		[Display(Name = "Тип иллюстрации")]
		public ProductImageType ImageType { get; set; }

		[Display(Name = "Тип иллюстрации")]
		public string DisplayImageType => ImageType.GetDisplayName();
	}
}
