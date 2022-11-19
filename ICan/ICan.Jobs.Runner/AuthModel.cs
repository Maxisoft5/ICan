using System.ComponentModel.DataAnnotations;

namespace ICan.Jobs.Runner
{
	public class AuthModel
	{
		public string Email { get; set; }
		[Display(Name = "Пароль")]
		public string Password { get; set; }
	}
}
