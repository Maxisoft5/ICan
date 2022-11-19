using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class ProductModel : IValidatableObject
	{
		public int ProductId { get; set; }

		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Display(Name = "Название")]
		public string Name { get; set; }
		
		public string DisplayName => string.IsNullOrWhiteSpace(CountryPrefix)
				? Name
				: $"{CountryPrefix} {Name}";

		public string ForeignDisplayName =>  !CountryId.HasValue
				? Name
				: $"{RegionalName}({Name})";

		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Display(Name = "Вид товара")]
		public int ProductKindId { get; set; }

		[Display(Name = "Набор")]
		public bool IsKit { get; set; }

		public bool AssemblesAsKit => IsKit || Const.AssemblesAsKitSeriesIds.Contains(ProductSeriesId ?? 0)
			|| Const.BoboProductIds.Contains(ProductId);

		[Display(Name = "Доступен")]
		public bool Enabled { get; set; } = true;

		public bool IsArchived { get; set; }

		public bool IsNoteBook => ProductKindId == 1;

		[Display(Name = "Серия")]
		public int? ProductSeriesId { get; set; }

		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Display(Name = "Вес, кг")]
		[Range(0, Const.MaxProductValue, ErrorMessage = Const.ValidationMessages.MinMaxLengthViolation)]
		public float Weight { get; set; }

		[Display(Name = "Вид товара")]
		public string ProductKindName { get; set; }

		[Display(Name = "Серия")]
		public string ProductSeriesName { get; set; }

		[Display(Name = "ISBN")]
		[StringLength(13)]
		public string ISBN { get; set; }
	
		[Display(Name = "Артикул")]
		public string ArticleNumber { get; set; }

		[Display(Name = "Порядок показа")]
		[Range(1, int.MaxValue, ErrorMessage = Const.ValidationMessages.PositiveFieldMessage)]
		public int? DisplayOrder { get; set; }

		public int Amount { get; set; } = 0;
		public double TotalSum { get; set; } = 0;

		public double? Price { get; set; }

		public int ActingAmount
		{
			get
			{
				if (IsNoteBook)
				{
					if (!IsKit || (KitProducts == null) || !KitProducts.Any())
						return 1;

					var kitSize = KitProducts.Where(kitP => kitP.ProductEnabled).Count();
					return kitSize;
				}
				else
				{
					return 0;
				}

			}
		}

		[Display(Name = "Региональное название")]
		public string RegionalName { get; set; }
		[Display(Name = "Страна")]
		public int? CountryId { get; set; }


		[Display(Name = "Показывать предыдущую цену")]
		public bool ShowPreviousPrice { get; set; }

		public string CountryPrefix { get; set; }
		public IEnumerable<ProductpriceModel> ProductPrices { get; set; }
		public IEnumerable<SemiproductModel> Semiproducts { get; set; }

		public IEnumerable<KitProductModel> KitProducts { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (ProductKindId == Const.NoteBookProductKind && string.IsNullOrWhiteSpace(ISBN))
				yield return new ValidationResult("Поле ISBN для тетрадей обязательно",
					new List<string> { nameof(ISBN) });	
			
			if (CountryId.HasValue && string.IsNullOrWhiteSpace(RegionalName))
				yield return new ValidationResult("Региональное название обязательно для заполнения, если выбрана страна",
					new List<string> { nameof(RegionalName) });
		}
		public IEnumerable<SemiproductProductRelationModel> RelatedSemiproducts { get; set; }
		public double? PreviousPrice { get; set; }
	}
}
