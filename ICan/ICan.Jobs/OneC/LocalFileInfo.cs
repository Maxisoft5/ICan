using System;
using System.Globalization;

namespace ICan.Jobs.OneC
{
	public class LocalFileInfo
	{
		public LocalFileInfo() { }
		public LocalFileInfo(string fileName, DateTime date)
		{
			this.FileName = fileName;
			this.Date = date;
		}

		public string FileName { get; set; }
		public DateTime Date { get; set; }

		public static LocalFileInfo Build(string item)
		{
			var dateStr = item.Substring(0, item.IndexOf(' '));
			var left = item.Substring(dateStr.Length + 1).Trim();

			var timeStr = left.Substring(0, left.IndexOf(' '));
			left = left.Substring(timeStr.Length + 1).Trim();

			var sizeStr = left.Substring(0, left.IndexOf(' '));
			left = left.Substring(sizeStr.Length + 1).Trim();

			var name = left;
			var uploadDate = DateTime.TryParseExact($"{dateStr} {timeStr}", "MM-dd-yy hh:mmtt", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var d) ? d : DateTime.MinValue;

			var fileInfo = new LocalFileInfo
			{
				Date = uploadDate,
				FileName = name
			};
			return fileInfo;
		}
	}
}