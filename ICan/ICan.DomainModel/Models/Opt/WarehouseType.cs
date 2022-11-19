using ICan.Common.Models.Enums;
using ICan.Common.Utils;
using ICan.Business.Services;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class WarehouseTypeModel
	{
		public int WarehouseTypeId { get; set; }

		[Display(Name="Название")]
		public string Name { get; set; }

		public string Comment { get; set; }

		[Display(Name = "Склад готовой продукции")]
		public bool ReadyToUse { get; set; }

		public WhJournalObjectType WarehouseObjectType { get; set; }


		[Display(Name = "Объект")]
		public string WarehouseObjectTypeDisplayName => WarehouseObjectType.GetDisplayName();

		public int? CounterpartyId { get; set; }


		[Display(Name = "Контрагент")]
		public string CounterpartyName { get; set; }
	}
}
