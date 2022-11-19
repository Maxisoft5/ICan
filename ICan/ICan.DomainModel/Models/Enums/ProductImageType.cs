using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Enums
{
	public enum ProductImageType
	{
		[Display(Name = "-")]
		None = 0,

		[Display(Name = "Каталог")]
		Catalog = 1,

		[Display(Name = "Галерея")]
		SmallGallery = 2,

		[Display(Name = "Большая галерея")]
		BigGallery = 3,

		[Display(Name = "Иллюстрация для текста")]
		TextImage = 4,

		[Display(Name = "Rich Content Иллюстрация")]
		RichContent = 5,
			
		[Display(Name = "Каталог для мобильного")]
		MobileCatalog = 6,
		
		[Display(Name = "Превью материала")]
		MaterialPreview = 7,
		
		[Display(Name = "Материал")]
		Material = 8,

		[Display(Name = "AliExpress")]
		AliExpress = 9,
	}
}
