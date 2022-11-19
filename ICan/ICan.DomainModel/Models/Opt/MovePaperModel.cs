using System;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class MovePaperModel
	{
		public int MovePaperId { get; set; }
		[Display(Name = "Дата перемещения")]
		[DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
		public DateTime MoveDate { get; set; }
		[Display(Name = "Отправитель")]
		public int SenderWarehouseId { get; set; }
		[Display(Name = "Получатель")]
		public int ReceiverWarehouseId { get; set; }
		[Display(Name = "Кол-во листов")]
		[Range(1, int.MaxValue, ErrorMessage = "Значение должно быть больше 0")]
		public int SheetCount { get; set; }
		[Range(0, double.MaxValue, ErrorMessage = "Значение должно быть больше 0")]
		[Display(Name = "Кол-во кг")]
		public double Weight { get; set; }
		[Display(Name = "Бумага")]
		public int PaperId { get; set; }
		[Display(Name = "Комментарий")]
		public string Comment { get; set; }

		[Display(Name = "Отправитель")]
		public string SenderName { get; set; }
		[Display(Name = "Получатель")]
		public string ReceiverName { get; set; }
		[Display(Name = "Бумага")]
		public string PaperName { get; set; }
		[Display(Name = "Заказ печати")]
		public int? PrintOrderPaperId { get; set; }
		public string PrintOrderName { get; set; }
		public bool? IsExpensePlanCovered { get; set; }
	}
}
