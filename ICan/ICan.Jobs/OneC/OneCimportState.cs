using System.Collections.Generic;

namespace ICan.Jobs.OneC
{
	public class OneCImportState
	{
		public int TotalAmount { get; set; }
		public int ProcessedAmount => ProcessedFileNames.Count;
		public List<string> ProcessedFileNames { get; set; } = new List<string>();

		public int DoneAmount => DoneFileNames.Count;
		public List<string> DoneFileNames { get; set; } = new List<string>();


		public int SkippedAmount => SkippedFileNames.Count;
		public List<string> SkippedFileNames { get; set; } = new List<string>();

		public int VirtSkippedAmount => VirtSkippedFileNames.Count;
		public List<string> VirtSkippedFileNames { get; set; } = new List<string>();

		public int UnknownShopAmount => UnknownShopFileNames.Count;
		public List<string> UnknownShopFileNames { get; set; } = new List<string>();

		public int RemovedAmount => RemovedFileNames.Count;
		public List<string> RemovedFileNames { get; set; } = new List<string>();

		public int ErrorAmount => ErrorFileNames.Count;
		public List<string> ErrorFileNames { get; set; } = new List<string>();


	}
}