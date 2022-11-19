using ICan.Common.Domain;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ICan.Common.Models
{
	// Add profile data for application users by adding properties to the ApplicationUser class
	public class ApplicationUser : IdentityUser
	{
		public ApplicationUser()
		{
			OptOrder = new HashSet<OptOrder>();
			ApplicationUserShopRelations = new HashSet<OptApplicationUserShopRelation>();
		}

		[Display(Name = "Имя")]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		[StringLength(0, ErrorMessage = Const.ValidationMessages.MaxLengthExceeded)]
		public string FirstName { get; set; }

		[Display(Name = "Фамилия")]
		[StringLength(0, ErrorMessage = Const.ValidationMessages.MaxLengthExceeded)]
		public string LastName { get; set; }

		[Display(Name = "Клиент")]
		public string FullName => $"{LastName} {FirstName}";

		[Display(Name = "Клиент")]
		public bool IsClient { get; set; }

		[JsonIgnore]
		public IEnumerable<OptOrder> OptOrder { get; set; }

		public string ShopsName => ApplicationUserShopRelations.Any() ? 
							string.Join(", ", ApplicationUserShopRelations.Select(x => x.Shop).Select(x => x.Name)) : "";

		[Display(Name = "Тип клиента")]
		public int ClientType { get; set; }

		[Display(Name = "Дата регистрации")]
		[DataType(DataType.Date)]
		public DateTime DateRegistration { get; set; }

		[JsonIgnore]
		public ICollection<OptApplicationUserShopRelation> ApplicationUserShopRelations { get; set; }
	}
}
