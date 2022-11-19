using ICan.Controllers;
using ICan.Common.Domain;
using System;
using System.Collections.Generic;
using Xunit;
using ICan.Common.Models.Opt;
using ICan.Common;

namespace ICan.Test.Order
{
	public class OrderModelStateTest
	{
		[Theory, MemberData(nameof(DataFor_CheckOrderModel_ChangeOrderStatusIfNotAdmin_IsInvalid))]
		public void CheckOrderModel_ChangeOrderStatusIfNotAdmin_IsInvalid(bool isAdmin, bool isOperator, int errorsCount)
		{
			var controller = GetOrderController();
			var model = new OrderModel { OrderStatusId = 3 };
			var optModel = new OptOrder { OrderStatusId = 2 };
			controller.CheckOrderModel(model, optModel, isAdmin, isOperator);

			var countErrorsActual = controller.ModelState.ErrorCount;

			Assert.Equal(errorsCount, countErrorsActual);
		}

		public static IEnumerable<object[]> DataFor_CheckOrderModel_ChangeOrderStatusIfNotAdmin_IsInvalid =>
		   new List<object[]>
		   {
					new object[] { true, true, 0 },
					new object[] { false, false, 1 },
					new object[] { true, false, 0 },
					new object[] { false, true, 0 },
		   };

		[Theory, MemberData(nameof(DataFor_CheckOrderModel_DateAssemblyLessOrderDate_IsInvalid))]
		public void CheckOrderModel_DateAssemblyLessOrderDate_IsInvalid(DateTime assemblyDate, DateTime orderDate, int errorsCount, string errorMessage)
		{
			var controller = GetOrderController();
			var model = new OrderModel { AssemblyDate = assemblyDate};
			var optModel = new OptOrder { OrderDate = orderDate, AssemblyDate = assemblyDate.AddDays(-1) };

			controller.CheckOrderModel(model, optModel, true, false);
			
			var countErrorsActual = controller.ModelState.ErrorCount;
			var errorMessageActual = countErrorsActual > 0 ? controller.ModelState[Const.ValidationMessages.DateAssemblyErrorKey].Errors[0].ErrorMessage : string.Empty;
			
			Assert.Equal(errorsCount, countErrorsActual);
			Assert.Equal(errorMessage, errorMessageActual);
		}

		public static IEnumerable<object[]> DataFor_CheckOrderModel_DateAssemblyLessOrderDate_IsInvalid =>
		   new List<object[]>
		   {
					new object[] { DateTime.Now.AddDays(-3), DateTime.Now, 1, Const.ValidationMessages.DateAssemblyErrorMessage },
					new object[] { DateTime.Now.Date, DateTime.Now.Date, 0, "" },
					new object[] { DateTime.Now.AddDays(3), DateTime.Now, 0, "" },
		   };


		[Theory, MemberData(nameof(DataFor_CheckOrderModerl_SetIsPaidIfNotAdmin_IsInvalid))]
		public void CheckOrderModerl_SetIsPaidIfNotAdmin_IsInvalid(bool IsPaid, bool IsAdmin, int expectedCountErrors, string errorMessage)
		{
			var controller = GetOrderController();
			var model = new OrderModel { IsPaid = IsPaid };
			var optModel = new OptOrder { IsPaid = !IsPaid };

			controller.CheckOrderModel(model, optModel, IsAdmin, IsAdmin);
			
			var countErrorsActual = controller.ModelState.ErrorCount;
			var errorMessageActual = countErrorsActual > 0 ? controller.ModelState[Const.ValidationMessages.ForbiddenIsPaidErrorKey].Errors[0].ErrorMessage : string.Empty;

			Assert.Equal(expectedCountErrors, countErrorsActual);
			Assert.Equal(errorMessage, errorMessageActual);
		}

		public static IEnumerable<object[]> DataFor_CheckOrderModerl_SetIsPaidIfNotAdmin_IsInvalid =>
		   new List<object[]>
		   {
						new object[] { true, true, 0, "" },
						new object[] { true, false, 1, Const.ValidationMessages.ForbiddenIsPaidErrorMessage },
		   };


		[Theory, MemberData(nameof(DataFor_CheckOrderModel_ChangeDoneDateByAdmin_IsValid))]
		public void CheckOrderModel_ChangeDoneDateByAdmin_IsValid(DateTime doneDate, bool roles, int expectedCountErrors, string errorMessage)
		{
			var controller = GetOrderController();
			var model = new OrderModel { DoneDate = doneDate };
			var optModel = new OptOrder { DoneDate = doneDate.AddDays(-1) };

			controller.CheckOrderModel(model, optModel, roles, roles);
			
			var countErrorsActual = controller.ModelState.ErrorCount;
			var errorMessageActual = countErrorsActual > 0 ? controller.ModelState[Const.ValidationMessages.DoneDateChangeForbiddenErrorKey].Errors[0].ErrorMessage : string.Empty;

			Assert.Equal(expectedCountErrors, countErrorsActual);
			Assert.Equal(errorMessage, errorMessageActual);
		}

		public static IEnumerable<object[]> DataFor_CheckOrderModel_ChangeDoneDateByAdmin_IsValid =>
		   new List<object[]>
		   {
					new object[] { DateTime.Now, true, 0, "" },
					new object[] { DateTime.Now, false, 1, Const.ValidationMessages.DoneDateChangeForbiddenErrorMessage },
		   };

		[Theory, MemberData(nameof(DataFor_CheckOrderModel_DoneDateLessDateOrder_IsInvalid))]
		public void CheckOrderModel_DoneDateLessDateOrder_IsInvalid(DateTime doneDate, DateTime orderDate, int errorsCount, string errorMessage, bool isSameError)
		{
			//arrange
			var controller = GetOrderController();
			var model = new OrderModel { DoneDate = doneDate, OrderDate = orderDate };
			var optModel = new OptOrder { OrderDate = orderDate };

			//act
			controller.CheckOrderModel(model, optModel, true, false);
			var errorsCountActual = controller.ModelState.ErrorCount;
			var isSameErrorActual = errorsCountActual > 0 ? controller.ModelState[Const.ValidationMessages.DoneDateErrorKey].Errors[0].ErrorMessage.Equals(errorMessage) : false;

			//asserts
			Assert.Equal(errorsCount, errorsCountActual);
			Assert.Equal(isSameError, isSameErrorActual);
		}

		public static IEnumerable<object[]> DataFor_CheckOrderModel_DoneDateLessDateOrder_IsInvalid =>
		   new List<object[]>
		   {
				new object[] { DateTime.Now.AddDays(-3), DateTime.Now, 1, Const.ValidationMessages.DoneDateErrorMessage, true },
				new object[] { DateTime.Now.AddDays(-1), DateTime.Now, 1, "", false },
				new object[] { DateTime.Now, DateTime.Now, 0, null, false },
				new object[] { DateTime.Now.AddDays(3), DateTime.Now, 0, null, false },
		   };


		private OrderController GetOrderController()
		{	
			return new OrderController(null, null, null, null,null,
				null, null, null, null);
		}	
	}
}
