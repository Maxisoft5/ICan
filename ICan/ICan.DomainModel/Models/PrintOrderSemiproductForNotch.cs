using ICan.Common.Domain;
using System;
using System.Collections.Generic;

namespace ICan.Common.Models
{
	public class PrintOrderSemiproductForNotchComparer : IEqualityComparer<OptPrintOrderSemiproduct>
	{
		public bool Equals(OptPrintOrderSemiproduct x, OptPrintOrderSemiproduct y)
		{
			if (Object.ReferenceEquals(x, y)) return true;

			if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
				return false;

			return x.SemiproductId == y.SemiproductId;
		}

		// If Equals() returns true for a pair of objects
		// then GetHashCode() must return the same value for these objects.

		public int GetHashCode(OptPrintOrderSemiproduct printOrderSemiproduct)
		{
			//Check whether the object is null
			if (Object.ReferenceEquals(printOrderSemiproduct, null)) return 0;

			//Calculate the hash code for the product.
			return printOrderSemiproduct.SemiproductId;
		}
	}

}
