using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class WarehouseModel : IValidatableObject
	{
		public readonly int[] SoleWActionTypes = new[] {
			(int)WarehouseActionType.Arrival,
			(int)WarehouseActionType.KitAssembly,
			(int)WarehouseActionType.SingleInventory
		};

		public int WarehouseId { get; set; }

		[Display(Name = "Дата добавления")]
		[DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
		public DateTime DateAdd { get; set; }

		public string DateAddStr => DateAdd.ToString("dd.MM.yyyy HH:mm");

		[Display(Name = "Комментарий")]
		[DataType(DataType.Text)]
		public string Comment { get; set; }

		[Display(Name = "Действие")]
		public int WarehouseActionTypeId { get; set; }

		[Display(Name = "Тип склада")]
		public WarehouseType WarehouseTypeId { get; set; }

		[Display(Name = "Тип склада")]
		public WarehouseTypeModel WarehouseType { get; set; }

		[Display(Name = "Тип склада")]
		public string WarehouseTypeName { get; set; }

		[Display(Name = "Сборка")]
		public int? AssemblyId { get; set; }

		[Display(Name = "Действие")]
		public string WarehouseActionTypeName { get; set; }

		[Display(Name = "Дата сборки")]
		[DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
		public DateTime? AssemblyDate { get; set; }

		[Display(Name = "Дата сборки")]
		public string AssemblyDateStr => AssemblyDate?.ToShortDateString();

		public IEnumerable<SelectListItem> WarehouseActionTypes { get; set; }

		public IEnumerable<SelectListItem> Assemblies { get; set; }

		public IEnumerable<KeyValuePair<OptProductseries, List<ProductModel>>> Items { get; set; }
		public IEnumerable<PaperWarehouseModel> PaperItems { get; set; }
		public IEnumerable<SelectListItem> WarehouseTypes { get; set; }

		[Display(Name = "Единственная тетрадь")]
		public WarehouseItemModel SoleItem => SoleWActionTypes.Contains(WarehouseActionTypeId) && WarehouseItems != null && WarehouseItems.Any()
			 ? WarehouseItems.First() : null;

		[Display(Name = "Единственная тетрадь")]
		public string SoleItemName => SoleItem?.Product ?? string.Empty;

		[Display(Name = "Количество")]
		public int? SoleItemAmount => SoleItem?.Amount;

		public List<WarehouseItemModel> WarehouseItems { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (WarehouseActionTypeId == (int)WarehouseActionType.Arrival &&
			   !AssemblyId.HasValue)
			{
				yield return new ValidationResult(
				$"В случае прихода необходимо выбрать сборку",
				null);
			}
			if (WarehouseActionTypeId == (int)WarehouseActionType.KitAssembly
				&& WarehouseItems != null && WarehouseItems.Count(t => t.Amount > 0) > 1)
			{
				yield return new ValidationResult(
				$"Невозможно внести больше одной позиции в случае сборки комплекта",
				null);
			}
			if (WarehouseActionTypeId == (int)WarehouseActionType.KitAssembly && WarehouseItems != null && !WarehouseItems.Any(t => t.Amount > 0))
			{
				yield return new ValidationResult(
				$"Необходимо внести позицию в случае сборки комплекта",
				null);
			}
			if (WarehouseActionTypeId == (int)WarehouseActionType.SingleInventory && WarehouseItems.Count(t => t.Amount > 0) > 1)
			{
				yield return new ValidationResult(
				$"Для единичной инвентаризации можно внести только 1 позицию",
				new List<string> { nameof(WarehouseActionTypeId) });
			}
		}
	}
}
