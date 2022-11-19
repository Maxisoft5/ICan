using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public partial class PaperOrderModel : IValidatableObject
	{
		public int PaperOrderId { get; set; }

		[Display(Name = "Дата заказа")]
		[DataType(DataType.Date, ErrorMessage = "")]
		[Required]
		public DateTime OrderDate { get; set; }

		[Display(Name = "Тип бумаги")]
		[Required]
		public int PaperId { get; set; }

		[Display(Name = "Формат бумаги")]
		[Required]
		public int FormatId { get; set; }

		[Display(Name = "Количество листов")]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Range(1, int.MaxValue, ErrorMessage = Const.ValidationMessages.PositiveFieldMessage)]
		public int SheetCount { get; set; }

		[Display(Name = "Количество, кг")]
		public double Weight { get; set; }

		[Display(Name = "Стоимость листа")]
		[Required]
		public double SheetPrice { get; set; }

		[Display(Name = "Стоимость листа")]
		public string SheetPriceRoundedFormatted => Math.Round(SheetPrice, 2).ToString("N2", CultureInfo.CreateSpecificCulture("ru-RU"));

		[Display(Name = "Сумма заказа")]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Range(1, int.MaxValue, ErrorMessage = Const.ValidationMessages.PositiveFieldMessage)]
		public double OrderSum { get; set; }

		[Display(Name = "Сумма заказа")]
		public string OrderSumFormatted => OrderSum.ToString("N2", CultureInfo.CreateSpecificCulture("ru-RU"));


		[Display(Name = "Номер счёта")]
		[MaxLength(50, ErrorMessage = Const.ValidationMessages.MaxLengthExceeded)]
		public string InvoiceNum { get; set; }

		[Display(Name = "Дата счёта")]
		[DataType(DataType.Date)]
		public DateTime? InvoiceDate { get; set; }

		[Display(Name = "Дата оплаты")]
		[DataType(DataType.Date)]
		public DateTime? PaymentDate { get; set; }

		[Display(Name = "Оплачен")]
		public bool IsPaid { get; set; }

		[Display(Name = "Поставщик")]
		[Required]
		public int SupplierCounterPartyId { get; set; }

		[Display(Name = "Получатель")]
		public int? RecieverCounterPartyId { get; set; }

		[Display(Name = "Поставщик")]
		public string SupplierCounterPartyName { get; set; }

		[Display(Name = "Получатель")]
		public string RecieverCounterPartyName { get; set; }

		[Display(Name = "Поставщик")]
		public string SupplierReciever => $"{SupplierCounterPartyName}";

		[Display(Name = "Формат бумаги")]
		public string FormatName { get; set; }

		[Display(Name = "Тип бумаги")]
		public string PaperName { get; set; }

		public IEnumerable<PrintOrderPaperModel> PrintOrderPapers { get; set; }
		public IEnumerable<PaperOrderIncomingModel> PaperOrderIncomings { get; set; }

		[Display(Name = "Участвует в заказе")]
		public int? SheetsTakenAmount => PrintOrderPapers == null || !PrintOrderPapers.Any()
			? null
			: (int?)PrintOrderPapers.Sum(printingOrder => printingOrder.SheetsTakenAmount);

		[Display(Name = "Остаток")]
		public int? SheetsLeftAmount => SheetCount - (SheetsTakenAmount ?? 0);

		[Range(0, int.MaxValue)]
		[Display(Name = "Оплаченная сумма")]
		public double? PaidSum { get; set; }

		[Display(Name = "Комментарий")]
		public string Comment { get; set; }

		public bool CanEditPaperType => PaperOrderIncomings == null || !PaperOrderIncomings.Any();

		public override string ToString() =>
		  $"{PaperName} от {OrderDate.ToShortDateString()}, осталось {SheetsLeftAmount}";


		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var results = new List<ValidationResult>();
			if (PaidSum > OrderSum)
			{
				results.Add(new ValidationResult(
				   $"Оплаченная сумма не может быть больше, чем сумма заказа {OrderSum}.",
				   null));
			}
			return results;
		}

		public static double GetWeight(int sheetCount, int? length, int? width, int? density)
		{
			if (length == null || width == null || density == null)
				return 0;			
			return Math.Round((double)(sheetCount * ((double)length / 1000) * ((double)width / 1000) * density / 1000), 2);
		}

		public static int GetSheetCount(double weight, int? length, int? width, int? density)
		{
			if (length == null || width == null || density == null
				|| length == 0 || width == 0 || density == 0)
				return 0;

			return (int)Math.Round((double)(weight * 1000 / ((double)length/1000 * ((double)width /1000) * density)), 0);
		}
	}
}
