using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class EventModel : IValidatableObject
	{
		public int EventId { get; set; }

		[Display(Name = "Название")]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		public string Name { get; set; }

		[Display(Name = "Дата начала")]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]

		[DataType(DataType.DateTime)]
		public DateTime? StartDate { get; set; }

		[Display(Name = "Дата окончания")]
		[DataType(DataType.DateTime)]
		public DateTime? EndDate { get; set; }

		[Display(Name = "Активна")]
		public bool Enabled { get; set; } = true;

		[Display(Name = "Процент скидки")]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Range(0.01, 100, ErrorMessage = "Процент скидки задан некорректно. Может быть в диапазоне от 0.01 до 100")]
		public float DiscountPercent { get; set; }

		[Display(Name = "Описание")]
		[StringLength(500, ErrorMessage = Const.ValidationMessages.MaxLengthExceeded)]
		[DataType(DataType.MultilineText)]

		public string Description { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (EndDate < StartDate)
				yield return new ValidationResult("Дата окончания акции должна быть больше или совпадать с датой начала");

		}
	}
}
