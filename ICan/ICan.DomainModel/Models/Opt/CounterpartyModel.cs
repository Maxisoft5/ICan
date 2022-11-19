using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class CounterpartyModel
	{
		public CounterpartyModel()
		{
		}

		public int CounterpartyId { get; set; }

		[MaxLength(200)]
		[Display(Name = "Название")]
		public string Name { get; set; }

		public string Consignee { get; set; }

		[MaxLength(12, ErrorMessage = "Максимальная длина ИНН может быть 12 цифр")]
		[Display(Name = "ИНН")]
		public string Inn { get; set; }

		[Display(Name = "Активен")]
		public bool Enabled { get; set; }

		[Display(Name = "Роль")]
		public int PaperOrderRoleId { get; set; }

		[Display(Name = "Роль")]
		public string PaperOrderRoleName { get; set; }

		[Display(Name = "Отсрочка платежа")]
		public int? PaymentDelay { get; set; }
	}
}
