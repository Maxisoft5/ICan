using ICan.Common.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class CampaignModel
	{
		public int CampaignId { get; set; }
		[Display(Name = "Тип кампании")]
		public CampaignType CampaignType { get; set; }

		[Display(Name = "Тема")]
		public string Title { get; set; }
		[Display(Name = "Текст рассылки")]
		public string Text { get; set; }
		[Display(Name = "Дата создания")]
		public DateTime Date { get; set; } = DateTime.Now;
		[Display(Name = "Рассылка отправлена")]
		public bool IsSent { get; set; }
		[Display(Name = "Название кампании")]
		public string CampaignName { get; set; }
		public IEnumerable<ImageModel> Images { get; set; }
	}
}
