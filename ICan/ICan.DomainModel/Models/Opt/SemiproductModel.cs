using ICan.Common.Domain;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class SemiproductModel : IValidatableObject
	{
		public int SemiproductId { get; set; }

		[Display(Name = "Тетрадь")]
		public int ProductId { get; set; }

		[Display(Name = "Серия")]
		public int? ProductSeriesId { get; set; }

		[Display(Name = "Формат")]
		public int? FormatId { get; set; }

		[Display(Name = "Вид")]
		public int SemiproductTypeId { get; set; }

		[Display(Name = "Количество полос")]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Range(1, int.MaxValue, ErrorMessage = Const.ValidationMessages.PositiveFieldMessage)]
		public int StripNumber { get; set; }

		[Display(Name = "Название")]
		public string Name { get; set; }

		[Display(Name = "Название")]
		public string DisplayName =>
				!string.IsNullOrWhiteSpace(Name) ?
				$"{SemiproductTypeName} {ProductCountryPrefix } {SeriesName} {ProductDisplayName} {Name}"
				: $"{SemiproductTypeName} {ProductDisplayName}";

		[Display(Name = "Описание")]
		public string Description { get; set; }

		[Display(Name = "Формат")]
		public string FormatName { get; set; }

		public string ProductName { get; set; }

		public string ProductCountryPrefix { get; set; }

		[Display(Name = "Тетрадь")]
		public string ProductDisplayName => string.IsNullOrWhiteSpace(ProductCountryPrefix)
				? ProductName
				: $"{ProductCountryPrefix} {ProductName}";

		[Display(Name = "Вид")]
		public string SemiproductTypeName { get; set; }

		public string SeriesName { get; set; }

		[Display(Name = "С ВД лаком")]
		public bool HaveWDVarnish { get; set; }

		[Display(Name = "Стохастика")]
		public bool HaveStochastics { get; set; }

		public string NameForAssembly => !string.IsNullOrWhiteSpace(Name) ? $"{Name} ({ProductDisplayName})" : $"{SemiproductTypeName} ({ProductDisplayName})";

		[Display(Name = "Длина реза, мм")]
		[RegularExpression("\\d+", ErrorMessage = "Значение поля \"{0}\" должно быть целым числом")]
		public int? CutLength { get; set; }

		[Display(Name = "Бумага")]
		public IEnumerable<OptSemiproductPaper> SemiproductPapers { get; set; }

		[Display(Name = "Бумага")]
		public string PaperNames => SemiproductPapers != null && SemiproductPapers.Any() ? string.Join(",", SemiproductPapers.Select(pap => pap.Paper?.Name)) : string.Empty;

		[Display(Name = "Универсальный")]
		public bool IsUniversal { get; set; }
		public IEnumerable<SemiproductProductRelationModel> RelatedProducts { get; set; }
		public int? LeftAmount { get; set; }
		[Display(Name = "Тип блока")]
		public int? BlockTypeId { get; set; }
		[Display(Name = "Тип блока")]
		public string BlockTypeName { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (SemiproductPapers == null || !SemiproductPapers.Any())
			{
				yield return new ValidationResult(
					$"Невозможно сохранить полуфабрикат без бумаги",
					new[] { nameof(SemiproductPapers) });
			}
		}
	}
}
