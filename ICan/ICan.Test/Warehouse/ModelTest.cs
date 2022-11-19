using ICan.Common.Models.Enums;
using ICan.Common.Domain;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;
using ICan.Common.Models.Opt;

namespace ICan.Test.Warehouse
{
	public class ModelTest
	{
		[Fact]
		public void WareouseModel_ActionIsArrivalWithoutAssembly_IsInvalid()
		{
			var model = new WarehouseModel { WarehouseActionTypeId = (int)WarehouseActionType.Arrival };

			var validationResultList = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(model, new ValidationContext(model), validationResultList);

			Assert.False(isValid);
		}
	}
}
