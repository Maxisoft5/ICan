using ICan.Common.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ICan.Common.Models.Opt
{
	public class ReportModel
	{
		public string ReportId { get; set; }
		public int ReportKindId { get; set; }
		public int ShopId { get; set; }

		[Display(Name = "Магазин")]
		public string ShopName { get; set; }

		[Display(Name = "Название файла")]
		public string FileName { get; set; }

		[Display(Name = "Вид отчёта")]
		public string ReportKindName { get; set; }

		[Display(Name = "Дата загрузки")]
		public DateTime UploadDate { get; set; }

		public int ReportMonth { get; set; }
		[Display(Name = "Месяц отчёта")]
		public string Month { get; set; }

		[Display(Name = "Год отчёта")]
		public int ReportYear { get; set; }

		[Display(Name = "Дата отчёта")]
		public DateTime? ReportDate { get; set; }

		[Display(Name = "Номер отчёта")]
		public long? ReportNum { get; set; }

		[Display(Name = "Вирт")]
		public bool? IsVirtual { get; set; }

		[Display(Name = "Общая сумма")]
		public double TotalSum { get; set; }
	
		[Display(Name = "Общая сумма")]
		public string TotalSumFormatted => TotalSum.ToString("N2", CultureInfo.CreateSpecificCulture("ru-RU"));
	
		public Dictionary<OptProductseries, List<ProductModel>> Items { get; set; }
	}
}
