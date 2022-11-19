namespace ICan.Common.Models.Opt
{
	public enum SemiProductType
	{
		None = 0,
		Block = 1,
		Stickers = 2,
		Covers = 3,
		Box = 4,
		Cursor = 5,
		Pointer = 6
	}

	public static class SemiProductTypeEx
	{
		public static string GetName(this SemiProductType semiProductType)
		{
			switch (semiProductType)
			{

				case SemiProductType.Block:
					return "Блок";
				case SemiProductType.Stickers:
					return "Наклейки";
				case SemiProductType.Covers:
					return "Обложки";
				case SemiProductType.Box:
					return "Коробка";
				default:
					return string.Empty;
			}
		}
	}
}
