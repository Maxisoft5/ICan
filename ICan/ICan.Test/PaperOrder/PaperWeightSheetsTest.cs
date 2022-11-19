using ICan.Common.Models.Opt;
using System.Collections.Generic;
using Xunit;

namespace ICan.Test.PaperCalc
{
	public class PaperWeightSheetsTest
	{
		[Theory, MemberData(nameof(DataFor_PaperOrderModel_GetWeightTest))]
		public void PaperOrderModel_GetWeightTest(int sheetCount, int length, int width, int density, double weightKG)
		{
			var weight = PaperOrderModel.GetWeight(sheetCount, length, width, density);
			Assert.Equal(weightKG, weight);
		}

		[Theory, MemberData(nameof(DataFor_PaperOrderModel_GetWeightTest))]
		public void PaperOrderModel_GetSheetsCountTest(int sheetCount, int length, int width, int density, double weightKG)
		{
			var sheets = PaperOrderModel.GetSheetCount(weightKG, length, width, density);
			Assert.Equal(sheetCount, sheets);
		}

		public static IEnumerable<object[]> DataFor_PaperOrderModel_GetWeightTest =>
		   new List<object[]>
		   {
				new object[] { 4, 620, 880, 115, 0.25 },
				new object[] { 4, 620, 880, 150, 0.33 },
				new object[] { 500, 64, 90, 170, 0.49 },
				new object[] { 4582, 62, 88, 120, 3 }
		   };
	}
}
