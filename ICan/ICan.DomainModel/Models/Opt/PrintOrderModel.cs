using ICan.Common.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public partial class PrintOrderModel
	{
		public int PrintOrderId { get; set; }

		[Display(Name = "Номер заказа в типографии")]
		public string PrintingHouseOrderNum { get; set; }

		[Display(Name = "Дата заказа")]
		[DataType(DataType.DateTime)]

		public DateTime OrderDate { get; set; }

		[Display(Name = "Тираж")]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Range(1, int.MaxValue, ErrorMessage = Const.ValidationMessages.PositiveFieldMessage)]
		public int Printing { get; set; }

		[Display(Name = "Запланированный расход")]
		[Range(1, int.MaxValue, ErrorMessage = Const.ValidationMessages.PositiveFieldMessage)]
		public int? PaperPlannedExpense { get; set; }

		[Display(Name = "Расхождение")]
		public int? PaperExpenseDifference => !PaperPlannedExpense.HasValue
			? null
			: PaperPlannedExpense - (PrintOrderPapers?.Sum(paper => paper.SheetsTakenAmount) ?? 0);


		[Display(Name = "Тип полуфабриката")]
		public int? SemiProductTypeId { get; set; }

		[Display(Name = "Тип полуф-та")]
		public string SemiProductType => SemiProductTypeId.HasValue
			? Enum.Parse<SemiProductType>(SemiProductTypeId.Value.ToString()).GetName()
			: string.Empty;

		[Display(Name = "Полуфабрикаты")]
		public List<PrintOrderSemiproductModel> PrintOrderSemiproducts { get; set; }

		public List<PrintOrderIncomingModel> PrintOrderIncomings { get; set; }

		public IEnumerable<IncomePrintingModel> MinIncomes
		{
			get
			{
				var incomings = (PrintOrderIncomings?.SelectMany(incoming => incoming.PrintOrderIncomingItems) ?? Enumerable.Empty<PrintOrderIncomingItemModel>());
				var minValues = new List<IncomePrintingModel>();

				foreach(var printOrderSm in PrintOrderSemiproducts)
				{
					var incoming = incomings.Where(inc => inc.PrintOrderSemiproductId == printOrderSm.PrintOrderSemiproductId)?.Sum(inc => inc.Amount) ?? 0;
					if (printOrderSm.SemiProduct == null)
						continue;
					var semiproductTypeId = (SemiProductType)printOrderSm.SemiProduct.SemiproductTypeId;
					var minValue = minValues.FirstOrDefault(mval => mval.SemiProductTypeId == semiproductTypeId);
					if (minValue != null)
					{
						minValue.MinIncome = (incoming < minValue.MinIncome) ? incoming : minValue.MinIncome;
					}
					else
					{
						minValues.Add(new IncomePrintingModel
						{
							MinIncome = incoming,
							SemiProductTypeId = semiproductTypeId,
							PrintingToCheck = Util.GetActingPrinting(Printing, (int)semiproductTypeId)
						});
					}
				};
				return minValues;
			}
		}


		[Display(Name = "Заказы бумаги")]
		public List<PrintOrderPaperModel> PrintOrderPapers { get; set; }

		[Display(Name = "Дата оплаты")]
		[DataType(DataType.Date)]
		public DateTime? PaymentDate { get; set; }

		[Display(Name = "Сумма заказа")]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[Range(1, int.MaxValue, ErrorMessage = Const.ValidationMessages.PositiveFieldMessage)]
		public double OrderSum { get; set; }

		[Display(Name = "Сумма заказа")]
		public string OrderSumFormatted => OrderSum.ToString("N2", CultureInfo.CreateSpecificCulture("ru-RU"));

		[Display(Name = "Оплачен")]
		public bool IsPaid { get; set; }

		[Display(Name = "Расход бумаги")]
		public int? SheetsTaken => PrintOrderPapers != null && PrintOrderPapers.Any() ?
		PrintOrderPapers.Sum(printOrderPaper => printOrderPaper.SheetsTakenAmount) : (int?)null;

		public bool AllPaperOrdersAreSent => PrintOrderPapers != null && PrintOrderPapers.Any() ?
					PrintOrderPapers.All(printOrderPaper => printOrderPaper.IsSent) : false;

		[Display(Name = "Название бумаги")]
		public string PaperName { get; set; }

		[Display(Name = "Примечание")]
		public string Comment { get; set; }


		[Display(Name = "Собран")]
		public bool IsAssembled { get; set; }

		[Display(Name = "ВД Лак")]
		public string HaveWDVarnish { get; set; }

		[Display(Name = "Стохастика")]
		public string HaveStochastics { get; set; }

		[Display(Name = "Подтверждаю печать")]
		public bool ConfirmPrint { get; set; }

		[Display(Name = "Номер счёта")]
		public string CheckNumber { get; set; }

		public string DisplayName => $"Заказ № {PrintingHouseOrderNum} от {OrderDate.ToShortDateString()}, тираж {Printing}";

		public IEnumerable<PrintOrderPaymentModel> PrintOrderPayments { get; set; }
		public decimal? PaidSum => PrintOrderPayments != null && PrintOrderPayments.Any()
			? PrintOrderPayments.Sum(payment => payment.Amount) : (decimal?)null;

		public string PaymentDiffSum => PaidSum.HasValue ? (Convert.ToDecimal(OrderSum) - PaidSum.Value).ToString("F0") : "";

		public bool CanEditSemiproducts { get; set; }
		public bool CanDeleteIncomes { get; set; }
		public bool HaveBlock => PrintOrderSemiproducts.Any(x => x.SemiProduct != null && x.SemiProduct.SemiproductTypeId == (int)Opt.SemiProductType.Block);
		public string BlockTypeName => PrintOrderSemiproducts.First().SemiProduct?.BlockTypeName;
		public IEnumerable<AssemblyModel> Assemblies { get; set; }
		public IEnumerable<NotchOrderModel> NotchOrders { get; set; } 
	}
}
