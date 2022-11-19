namespace ICan.Common.Models.Opt
{
	public class PrintOrderAssemblyInfo
	{
		public bool CanAssembly
		{
			get
			{
				if (SkipCheck)
					return true;
				if (IsCalendar)
					return (IsEnoughBlocksAtWarehouse && IsEnoughBlocksInPrintOrderIncoming);
				return (IsEnoughBlocksAtWarehouse
				&& IsEnoughBlocksInPrintOrderIncoming
				&& IsEnoughStickersAtWarehouse
				&& ((ConsiderNotch && IsEnoughStickersInNotchOrderIncoming) || IsEnoughStickersInPrintOrderIncoming)
				&& IsEnoughCoversAtWarehouse
				&& IsEnoughCoversInPrintOrderIncoming);
			}
		}

		public bool SkipCheck { get; set; }
		public bool ConsiderNotch { get; set; }
		public bool IsEnoughBlocksAtWarehouse { get; set; }
		public bool IsEnoughBlocksInPrintOrderIncoming { get; set; }
		public bool IsEnoughStickersAtWarehouse { get; set; }

		public bool IsEnoughStickersInPrintOrderIncoming { get; set; }
		public bool IsEnoughStickersInNotchOrderIncoming { get; set; }
		public bool IsEnoughCoversAtWarehouse { get; set; }
		public bool IsEnoughCoversInPrintOrderIncoming { get; set; }
		public int ProductSeriesId { get;  set; }
		public bool IsCalendar => Const.CalendarSeriedId == ProductSeriesId;
		public bool NeedChekCovers => !IsCalendar;
		public bool NeedCheckStickers => !IsCalendar;
	}
}