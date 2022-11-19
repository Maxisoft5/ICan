namespace ICan.Common.Models.Opt.Report
{
	public class ExcelNavigator
	{
		public int Row { get; set; }
		public int Col { get; set; }
		public string Text { get; set; }

		public ExcelNavigator(int row, int col, string text)
		{
			Row = row;
			Col = col;
			Text = text;
		}
	}
}
