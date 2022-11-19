using ICan.Common.Domain;
using ICan.Common.Models.Opt.Report;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ICan.Test.ParseReports
{
	public class DbContextFixture
	{
		public ApplicationDbContext Context { get; private set; }

		public DbContextFixture()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase("Testing")
				.Options;
			Context = new ApplicationDbContext(options);
			Context.OptReportCriteriaGroups.AddRange(GetReportCriteriaGroups());
			Context.OptReportCriteria.AddRange(GetReportCriteria());
			Context.OptProduct.AddRange(GetProducts());
			Context.OptShopName.AddRange(GetShopNames());
			Context.SaveChanges();
		}

		private IEnumerable<OptReportCriteriaGroup> GetReportCriteriaGroups()
		{
			var reportCriteriaGroups = new List<OptReportCriteriaGroup> {
					new OptReportCriteriaGroup{ GroupId = 1, ShopId = 4, Type = 1, GroupName = "ReadingCityCriteria", IsMain = false, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 2, ShopId = 2, Type = 1, GroupName = "WBCriteria", IsMain = false, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 3, ShopId = 1, Type = 1, GroupName = "LabyrinthCriteria", IsMain = false, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 4, ShopId = 7, Type = 1, GroupName = "OzonCriteria", IsMain = false, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 5, ShopId = 6, Type = 1, GroupName = "MyShopCriteria", IsMain = false, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 6, ShopId = 7, Type = 2, GroupName = "DateInformation ", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 7, ShopId = 7, Type = 3, GroupName = "ReportNumberInformation", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 8, ShopId = 7, Type = 4, GroupName = "TotalSumInformation", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 9, ShopId = 7, Type = 5, GroupName = "BookAmountInformation", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 10, ShopId = 7, Type = 6, GroupName = "BookIdInformation", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 11, ShopId = 7, Type = 4, GroupName = "TotalSumInformation", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 12, ShopId = 7, Type = 7, GroupName = "DateFormat", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 13, ShopId = 2, Type = 7, GroupName = "DateFormat", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 14, ShopId = 2, Type = 4, GroupName = "TotalSumInformation", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 15, ShopId = 2, Type = 5, GroupName = "BookAmountInformation", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 16, ShopId = 2, Type = 6, GroupName = "BookIdInformation", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 17, ShopId = 2, Type = 6, GroupName = "BookIdInformation", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 18, ShopId = 4, Type = 6, GroupName = "BookIdInformation", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 19, ShopId = 4, Type = 5, GroupName = "BookAmountInformation", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 20, ShopId = 4, Type = 4, GroupName = "TotalSumInformation", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 21, ShopId = 6, Type = 7, GroupName = "DateFormat", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 22, ShopId = 6, Type = 2, GroupName = "DateInformation", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 23, ShopId = 6, Type = 6, GroupName = "BookIdInformation", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 24, ShopId = 6, Type = 5, GroupName = "BookAmountInformation", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 25, ShopId = 6, Type = 4, GroupName = "TotalSumInformation", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 26, ShopId = 7, Type = 8, GroupName = "ReportItemTotalSum", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 27, ShopId = 2, Type = 8, GroupName = "ReportItemTotalSum", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 28, ShopId = 7, Type = 9, GroupName = "ComissionColumn", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 29, ShopId = 2, Type = 1, GroupName = "WBReportByPeriod", IsMain = true, ReportKind = ReportKind.ReportByPeriod },
					new OptReportCriteriaGroup{ GroupId = 30, ShopId = 2, Type = 10, GroupName = "ReportPeriodFrom", IsMain = true, ReportKind = ReportKind.Report },
					new OptReportCriteriaGroup{ GroupId = 31, ShopId = 2, Type = 11, GroupName = "ReportPeriodTo", IsMain = true, ReportKind = ReportKind.Report }
			};
			return reportCriteriaGroups;
		}
		private IEnumerable<OptReportCriteria> GetReportCriteria()
		{
			var result = new List<OptReportCriteria>
			{
				new OptReportCriteria{ CriteriaId = 1, GroupId = 1, Address = "B1", Information = "ISBN"},
				new OptReportCriteria{ CriteriaId = 2, GroupId = 1, Address = "A1", Information = "Поставщик"},
				new OptReportCriteria{ CriteriaId = 3, GroupId = 2, Address = "A14", Information = "Вайлдберриз"},
				new OptReportCriteria{ CriteriaId = 4, GroupId = 3, Address = "B1", Information = "Отчет о товарных остатках"},
				new OptReportCriteria{ CriteriaId = 6, GroupId = 4, Address = "B7", Information = "Интернет Решения"},
				new OptReportCriteria{ CriteriaId = 7, GroupId = 5, Address = "A2", Information = "Номенклатура"},
				new OptReportCriteria{ CriteriaId = 8, GroupId = 6, Address = "G3", Information = ""},
				new OptReportCriteria{ CriteriaId = 9, GroupId = 7, Address = "G2", Information = ""},
				new OptReportCriteria{ CriteriaId = 10, GroupId = 8, Address = "E", Information = ""},
				new OptReportCriteria{ CriteriaId = 11, GroupId = 9, Address = "U", Information = ""},
				new OptReportCriteria{ CriteriaId = 12, GroupId = 10, Address = "F", Information = ""},
				new OptReportCriteria{ CriteriaId = 13, GroupId = 11, Address = "E", Information = ""},
				new OptReportCriteria{ CriteriaId = 15, GroupId = 13, Address = "", Information = "yyyy-MM-dd"},
				new OptReportCriteria{ CriteriaId = 16, GroupId = 14, Address = "S", Information = ""},
				new OptReportCriteria{ CriteriaId = 17, GroupId = 15, Address = "HH", Information = ""},
				new OptReportCriteria{ CriteriaId = 18, GroupId = 16, Address = "F", Information = ""},
				new OptReportCriteria{ CriteriaId = 19, GroupId = 18, Address = "B", Information = ""},
				new OptReportCriteria{ CriteriaId = 20, GroupId = 19, Address = "F", Information = ""},
				new OptReportCriteria{ CriteriaId = 21, GroupId = 20, Address = "G", Information = ""},
				new OptReportCriteria{ CriteriaId = 22, GroupId = 12, Address = "", Information = "dd.MM.yyyy"},
				new OptReportCriteria{ CriteriaId = 23, GroupId = 21, Address = "", Information = "dd.MM.yyyy"},
				new OptReportCriteria{ CriteriaId = 24, GroupId = 22, Address = "C1", Information = ""},
				new OptReportCriteria{ CriteriaId = 25, GroupId = 23, Address = "J", Information = ""},
				new OptReportCriteria{ CriteriaId = 26, GroupId = 24, Address = "S", Information = ""},
				new OptReportCriteria{ CriteriaId = 27, GroupId = 25, Address = "R", Information = ""},
				new OptReportCriteria{ CriteriaId = 28, GroupId = 26, Address = "W", Information = ""},
				new OptReportCriteria{ CriteriaId = 29, GroupId = 27, Address = "S", Information = ""},
				new OptReportCriteria{ CriteriaId = 30, GroupId = 28, Address = "AA", Information = ""},
				new OptReportCriteria{ CriteriaId = 31, GroupId = 29, Address = "N2", Information = "Форма отчета"},
				new OptReportCriteria{ CriteriaId = 32, GroupId = 30, Address = "L7", Information = ""},
				new OptReportCriteria{ CriteriaId = 33, GroupId = 31, Address = "N7", Information = ""}
			};
			return result;
		}
		private IEnumerable<OptProduct> GetProducts()
		{
			var result = new List<OptProduct>
			{
				new OptProduct{ ProductId = 3, ProductKindId = 1, Name = "Я могу! Комплект из 5 пособий. Серия 2-3 года.", ISBN = "9785604001912"},
				new OptProduct{ ProductId = 4, ProductKindId = 1, Name = "Я могу рисовать линии!2 - 3 года.", ISBN = "9785990969087" },
				new OptProduct{ ProductId = 5, ProductKindId = 1, Name = "Я могу вырезать и клеить! 2 - 3 года.", ISBN = "9785990969018"},
				new OptProduct{ ProductId = 6, ProductKindId = 1, Name = "Я могу находить решения!2 - 3 года.", ISBN = "9785990969070"},
				new OptProduct{ ProductId = 7, ProductKindId = 1, Name = "Я могу запоминать! 2 - 3 года.", ISBN = "9785990969049"},
				new OptProduct{ ProductId = 8, ProductKindId = 1, Name = "Я могу проходить лабиринты!2 - 3 года.", ISBN = "9785990969056"},
				new OptProduct{ ProductId = 9, ProductKindId = 1, Name = "Я могу!Комплект из 5 пособий.Серия 3 - 4 года.", ISBN = "9785604001905"},
				new OptProduct{ ProductId = 10, ProductKindId = 1, Name = "Я могу рисовать линии!3 - 4 года.", ISBN = "9785990969001"},
				new OptProduct{ ProductId = 11, ProductKindId = 1, Name = "Я могу вырезать и клеить! 3 - 4 года", ISBN = "9785990969032"},
				new OptProduct{ ProductId = 12, ProductKindId = 1, Name = "Я могу находить решения!3 - 4 года.", ISBN = "9785990969063"},
				new OptProduct{ ProductId = 13, ProductKindId = 1, Name = "Я могу запоминать! 3 - 4 года.", ISBN = "9785990969094"},
				new OptProduct{ ProductId = 14, ProductKindId = 1, Name = "Я могу проходить лабиринты!3 - 4 года.", ISBN = "9785990969025"},
				new OptProduct{ ProductId = 15, ProductKindId = 1, Name = "Я могу вырезать и клеить! Живые картинки. 2 - 4 года", ISBN = "9785604001981"},
				new OptProduct{ ProductId = 16, ProductKindId = 1, Name = "Я могу!Комплект из 5 пособий.Серия 4 - 5 лет.", ISBN = "9785604001929"},
				new OptProduct{ ProductId = 17, ProductKindId = 1, Name = "Я могу изобретать! 4 - 5 лет.", ISBN = "9785604001936"},
				new OptProduct{ ProductId = 18, ProductKindId = 1, Name = "Я могу вырезать и клеить! 4 - 5 лет.", ISBN = "9785604001943"},
				new OptProduct{ ProductId = 19, ProductKindId = 1, Name = "Я могу находить решения!4 - 5 лет.", ISBN = "9785604001950"},
				new OptProduct{ ProductId = 20, ProductKindId = 1, Name = "Я могу запоминать! 4 - 5 лет.", ISBN = "9785604001967"},
				new OptProduct{ ProductId = 21, ProductKindId = 1, Name = "Я могу проходить лабиринты!4 - 5 лет.", ISBN = "9785604001974"},
				new OptProduct{ ProductId = 31, ProductKindId = 1, Name = "Я могу вырезать и клеить! Живые картинки. 4 - 6 лет.", ISBN = "9785604001998"},
				new OptProduct{ ProductId = 34, ProductKindId = 1, Name = "Я могу сделать календарь сам!", ISBN = "9785604166505"},
				new OptProduct{ ProductId = 35, ProductKindId = 1, Name = "Я могу вырезать и клеить! 5 - 6 лет.", ISBN = "9785604166598"},
				new OptProduct{ ProductId = 36, ProductKindId = 1, Name = "Я могу читать сам!2 ступень.", ISBN = "9785604166512"},
				new OptProduct{ ProductId = 46, ProductKindId = 1, Name = "Я могу читать сам!1 ступень.", ISBN = "9785604166581"},
				new OptProduct{ ProductId = 47, ProductKindId = 1, Name = "Букварь! Бобуквы", ISBN = "9785604166574"},
				new OptProduct{ ProductId = 48, ProductKindId = 1, Name = "Я могу читать сам!3 ступень.", ISBN = "9785604379301"},
				new OptProduct{ ProductId = 55, ProductKindId = 1, Name = "Букварь! Бобосказки", ISBN = "9785604379349"},
				new OptProduct{ ProductId = 56, ProductKindId = 1, Name = "Я могу лепить и рисовать! Картинки из пластилина. 2 - 3 года.", ISBN = "9785604379325"},
			};
			return result;
		}
		private IEnumerable<OptShopName> GetShopNames()
		{
			var result = new List<OptShopName>
			{
				new OptShopName{ ShopNameId = 46, ShopId = 1, Name = "ООО \"Торговый Дом Лабиринт\""},
				new OptShopName{ ShopNameId = 14, ShopId = 1, Name = "ООО \"Торговый Дом Лабиринт\""},
				new OptShopName{ ShopNameId = 23, ShopId = 11, Name = "ТОО «Happy Kingdom»"},
				new OptShopName{ ShopNameId = 25, ShopId = 10, Name = "ООО \"Книжная Нора\""},
				new OptShopName{ ShopNameId = 27, ShopId = 4, Name = "Общество с ограниченной ответственностью \"ПАРТНЕР АЙ ДИ\""},
				new OptShopName{ ShopNameId = 29, ShopId = 2, Name = "ООО \"Вайлдберииз\""},
				new OptShopName{ ShopNameId = 30, ShopId = 5, Name = "ЗАО \"Планета увлечений\""},
				new OptShopName{ ShopNameId = 31, ShopId = 6, Name = "ООО \"Букселлер\""},
				new OptShopName{ ShopNameId = 33, ShopId = 8, Name = "ООО \"Вэйтекс\""},
				new OptShopName{ ShopNameId = 34, ShopId = 9, Name = "ООО \"КнигоБалт\""},
				new OptShopName{ ShopNameId = 35, ShopId = 7, Name = "ООО \"Интернет Решения\""},
				new OptShopName{ ShopNameId = 36, ShopId = 12, Name = "ООО \"ТОРГОВЫЙ ДОМ \"ЭКСМО\""},
				new OptShopName{ ShopNameId = 38, ShopId = 2, Name = "ООО \"Вайлдберриз\""},
				new OptShopName{ ShopNameId = 39, ShopId = 14, Name = "ООО \"ЭКСПОТРЕЙД\""},
				new OptShopName{ ShopNameId = 40, ShopId = 1, Name = "ООО \"Торговая Фирма Лабиринт\""},
				new OptShopName{ ShopNameId = 41, ShopId = 15, Name = "ООО \"МИР СОВРЕМЕННЫХ ТЕХНОЛОГИЙ\""},
				new OptShopName{ ShopNameId = 42, ShopId = 16, Name = "ООО \"СПЕКТР ПРЕССЫ\""},
				new OptShopName{ ShopNameId = 43, ShopId = 17, Name = "ИП Минчинский Денис Геннадьевич"},
				new OptShopName{ ShopNameId = 44, ShopId = 18, Name = "ООО \"ВКУСВИЛЛ\""},
				new OptShopName{ ShopNameId = 45, ShopId = 19, Name = "ИП Омарбекова Елена Юрьевна"},
				new OptShopName{ ShopNameId = 47, ShopId = 20, Name = "ИП Минчинская Елена Владимировна"},
				new OptShopName{ ShopNameId = 48, ShopId = 21, Name = "ИП Пранович-Рославцева Наталия Валентиновна"}
			};
			return result;
		}
	}
}
