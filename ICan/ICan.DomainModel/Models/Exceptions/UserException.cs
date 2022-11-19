using System;

namespace ICan.Common.Models.Exceptions
{
	public class UserException : Exception
	{
		public UserException(string message) : base(message)
		{
		}
	}
}
