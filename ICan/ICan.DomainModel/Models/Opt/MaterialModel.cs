using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class MaterialModel
	{
		public int MaterialId { get; set; }
		[Display(Name="Дата публикации")]
		public DateTime Date { get; set; }
		[Display(Name = "Дата публикации")]
		public string DisplayDate => Date.ToString("dd.MM.yyyy");

		[Display(Name = "Тема")]
		public string Theme { get; set; }
		[Display(Name = "Содержание")]
		public string Content { get; set; }
		[Display(Name = "Активен")]
		public bool IsActive { get; set; }

		public IEnumerable<ImageModel>  Images { get; set; }
		public IEnumerable<ImageModel> PreviewImages => Images?.Where(img => img.ImageType == Enums.ProductImageType.MaterialPreview)
			?.OrderBy(image => image.Order);		
		
 
		
		public ImageModel DownloadFile => Images?.FirstOrDefault(img => img.ImageType == Enums.ProductImageType.Material);
	}
}
