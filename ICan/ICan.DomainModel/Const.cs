using ICan.Common.Models.AccountViewModels;
using System;
using System.Collections.Generic;

namespace ICan.Common
{
	public static class Const
	{
		public const int GluePadProductId = 100;
		public const int CalendarId = 34;
		public const int CalendarSeriesId = 10;
		public const int idColumn = 0;
		public const int RangeEnd = 10000;
		public const int CursorPrintingCoeff = 10;

		public const string Import1CJobName = "1C-job";
		public const string ImportWbSalesJobName = "WbSales-job";
		public const string ImportWbOrdersJobName = "WbOrders-job";
		public const string ImportWbSalesMonthsJobName = "WbMonthsSales-job";
		public const string ImportWbOrdersMonthsJobName = "WbMonthsOrders-job";
		public const int KitSize = 5;
		public const long ProductPromoId = 6;
		public const long OrderPromoId = 7;
		public const long DeliveryInfoId = 8;

		public const string SizeTemplate = "От {0} до {1}";
		public const string ShortSizeTemplate = "От {0}";

		public const int OtherPriceId = 5;
		public const int NoteBookProductKind = 1;
		public static readonly int CalendarSeriedId = 10;
		public static readonly int BobAlphabetId = 47;
		public static readonly int BobTalesId = 55;
		public static readonly int BobJokes = 129;

		public static readonly int[] AssemblesAsKitSeriesIds = new int[] { CalendarSeriedId /*Календарь*/ };
		public static readonly int[] BoboProductIds = new int[] { BobAlphabetId, /*Бобуквы*/ BobTalesId /*Бобосказки*/ , BobJokes};
		
		public static readonly int PageSize = 20;
		public const int MaxProductValue = 100;
		public const int MaxSpoolValue = 1000;
		public const string HeaderTemplate = "от ([0-9]*) пособий";
		public const string EndlessEventTitle = "Внимание! У нас проходит акция! Сейчас ваша дплнительная скидка на всю продукцию составит: {0}%";
		public const string EventTail = "1 комплект считается как 5 пособий";
		public const string OrderMessageTheme = "Оптовый заказ на сайте yamogu.ru # {0}";
		public const string NewTrackNo = "Трек номер вашего заказа {0} на сайте yamogu.ru -  {1}";

		public const string OrderMessageBody =
					 "Уважаемый клиент! <br />" +
					 "Ваш заказ принят, изменения можете вносить до начала сборки. <br />" +
					 "Инструкция по оплате: <br/> {0}  <br/>" +
					 "<br/>" +
					 "С уважением, <br/>" +
					 "Команда Я могу!";

		public const string NewTrackBody =
					"Уважаемый клиент! <br />" +
					"Трек номер вашего заказа {0} на сайте yamogu.ru -  {1}. <br />" +
					"<br/>" +
					"С уважением,  <br /> " +
					"Команда Я могу!";

		public const string EventTitle = "До {0} дополнительная скидка на всю продукцию {1}%";
		public const string TemporaryPassword = @"DhtvtyysqGfhjkm!";

		public const int WbDaysInScope = 7;

		public const int CommonSiteFilter = 1;

		public static readonly float? DefautRating = 5.0f;

		public static class JobName
		{
			public static string WbMarketplaceParse = "WbParsePricesJob";
			public static string OzonMarketplaceParse = "OzonMarketplaceParse";  
			public static string OzonApiPriceImportJob = "OzonApiPriceImportJob";
			public static string WbSalesApiImportJob = "WbSalesApiImportJob";
			public static string WbOrdersApiImportJob = "WbOrdersApiImportJob";
		}

		public static class ValidationMessages
		{
			public const string PositiveFieldMessage = "Поле \"{0}\" должно содержать положительное значение";
			public const string InvalidValue = "Поле \"{0}\" должно содержать корректное значение";
			public const string RequiredFieldMessage = "Поле \"{0}\" обязательно для заполнения";
			public const string MaxLengthExceeded = "Для поля \"{0}\" максимально допустимая длина {1} символов";
			
			public const string MinMaxRangeExceeded = "Для поля \"{0}\" минимально допустимое значение {1}, максимально допустимое значение {2}";

			public const string MinMaxLengthViolation = "Для поля \"{0}\" минимально допустимая длина {2}, максимально допустимая длина {1} символов";

			public const string DateAssemblyErrorKey = "DateAssemblyError";
			public const string DateAssemblyErrorMessage = "Дата сборки заказа не может быть меньше, чем дата заказа";

			public const string ForbiddenIsPaidErrorKey = "ForbiddenIsPaid";
			public const string ForbiddenIsPaidErrorMessage = "Ставить отметку \"Оплачено\" могут только пользователи с ролью \"Администратор\"";

			public const string DoneDateErrorKey = "DoneDateError";
			public const string DoneDateErrorMessage = "Дата выполнения заказа не может быть меньше, чем дата заказа";

			public const string DoneDateChangeForbiddenErrorKey = "ForbiddenChangeDoneDate";
			public const string DoneDateChangeForbiddenErrorMessage = "Не хватает прав для изменения даты выполнения заказа";

			public const string StatusChangeErrorKey = "ForbiddenChangeStatus";
			public const string StatusChangeErrorMessage = "Переводить в указанный статус могут только пользователи с ролью \"Оператор\" или \"Директор\"";
		}

		public static class ErrorMessages
		{
			public const string CantDeleteProductSeries = "Невозможно удалить {0}, существуют товары этой серии";
			public const string CantDeleteReport = "Невозможно удалить  отчёт за {0}";
			public const string CantDeleteWarehouse = "Невозможно удалить записи об остатках за {0}";
			public const string CantDeleteForUser = "Невозможно удалить запись";
			public const string CantDeleteTypeOfPaper = "Невозможно удалить вид бумаги до того, как удалены все бумаги указанного вида";
			public const string CantDeleteSpring = "Невозможно удалить пружину, так как она участвует в заказах пружины";

			public const string CantSaveOrder = "Невозможно сохранить заказ номер {0}";

			public const string CantSaveProduct = "Невозможно сохранить товар (№{0})";

			public const string CantSave = "Возникла ошибка при работе {0}, контроллер {1}, id {2}";
			public const string CantSaveForUser = "Невозможно сохранить информацию";
			public const string CantDeleteClient = "Невозможно удалить клиента {0}, так как у него есть заказы";
			public const string CantDelete = "Возникла ошибка при удалении записи с id {0}";
			public const string CantDeleteProduct = "Возникла ошибка при удалении товара с id {0}";

			public static string CantDeleteFormat = "Невозможно удалить формат, так как он используется  бумаге";

			public const string CantShowInfo = "Нет прав для просмотра информации";

			public const string NoFile = "Не приложен файл";
	 
			public const string CantRecognizeReportType = "Не удалось определить тип отчёта";

			public static string WrongNumberOfPackages = "Количество коробок в XLSX файле не соответствует количесту наклеек в PDF файле";
			public static string NoPackageInfo = "Не указано количество коробок для ";

			public static string CheckNumbers = "Невозможно сохраниь информацию. Проверьте внесённые данные";

			public static string UnsufficientRights = "Недостаточно прав для совершения операции";
			public static string CantStart = "Не удалось запустить задачу";

		}
		public static class SuccessMessages
		{
			public const string ProductWasDeleted = "Товар \"{0}\" был успешно удалён";
			public const string Deleted = "Информация успешно удалена";
			public const string Saved = "Информация успешно сохранена";
			public const string Started = "Задача запущена";
		}

		public static class Settings
		{
			public const string Delivery = "Settings:DeliveryFilePath";
		}

		public static class Roles
		{
			public static List<RoleDescription> RoleDescriptionList = new List<RoleDescription>
			{
				new RoleDescription{ Name = "Кладовщик", NameEn = StoreKeeper },
				new RoleDescription{ Name = "Сборщик", NameEn = Assembler },
				new RoleDescription{ Name = "Оператор", NameEn = Operator },
				new RoleDescription{ Name = "Дизайнер", NameEn = Designer },
				new RoleDescription{ Name = "Администратор", NameEn = Admin },
				new RoleDescription{ Name = "Контент менеджер", NameEn = ContentMan },
			};

			public const string StoreKeeper = "StoreKeeper";
			public const string Admin = "Admin";
			public const string Assembler = "Assembler";
			public const string Operator = "Operator";
			public const string Designer = "Designer";
			public const string ContentMan = "ContentMan";			
		}

		public static readonly TimeSpan DeletePeriod = TimeSpan.FromMinutes(10);
	}
}
