using System;

namespace ICan.Common.Models.Exceptions
{
	public class UnknownReportException : Exception
	{
		public UnknownReportException(string message) : base(message)
		{
		}
	}
}
