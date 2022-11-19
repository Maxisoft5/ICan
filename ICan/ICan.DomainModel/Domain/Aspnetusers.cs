using System;
using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public partial class Aspnetusers
	{
		public Aspnetusers()
		{
			OptOrder = new HashSet<OptOrder>();
			OptProductprice = new HashSet<OptProductprice>();
		}

		public string Id { get; set; }
		public int AccessFailedCount { get; set; }
		public string ConcurrencyStamp { get; set; }
		public string Email { get; set; }
		public bool EmailConfirmed { get; set; }
		public bool LockoutEnabled { get; set; }
		public DateTimeOffset? LockoutEnd { get; set; }
		public string NormalizedEmail { get; set; }
		public string NormalizedUserName { get; set; }
		public string PasswordHash { get; set; }
		public string PhoneNumber { get; set; }
		public bool PhoneNumberConfirmed { get; set; }
		public string SecurityStamp { get; set; }
		public bool TwoFactorEnabled { get; set; }
		public string UserName { get; set; }
		public string FirstName { get; set; }
		public string PatronymicName { get; set; }
		public string LastName { get; set; }

		public ICollection<OptOrder> OptOrder { get; set; }
		public ICollection<OptProductprice> OptProductprice { get; set; }

		public int? ClientType { get; set; }
	}
}
