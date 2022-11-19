using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Opt.Report;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace ICan.Business.Services
{
	using CriteriaDictionary = Dictionary<int, List<KeyValuePair<ReportKind, List<KeyValuePair<string, string>>>>>;

	public class ReportCriteriaService
	{
		private ApplicationDbContext _context;
		private static CriteriaDictionary _criteria;
		private static readonly object _criteriaLock = new object();

		private static IEnumerable<OptReportCriteriaGroup> _criteriaGroups;
		private static readonly object _criteriaGroupsLock = new object();

		private Dictionary<ShopType, Dictionary<ReportKind, List<ExcelNavigator>>> _criteriaXLS;
		private static readonly object _criteriaXLSLock = new object();

		public ReportCriteriaService(ApplicationDbContext context)
		{
			_context = context;
		}

		public CriteriaDictionary GetCriteria()
		{
			if (_criteria == null)
			{
				lock (_criteriaLock)
				{
					_criteria = GetCriteriaInternal();
				}
			}
			return _criteria;
		}

		public bool ClearCriteria()
		{
			lock (_criteria)
			{
				_criteria = null;
			}
			return true;
		}

		public Dictionary<ShopType, Dictionary<ReportKind, List<ExcelNavigator>>> GetXLSCriteria()
		{
			if (_criteriaXLS == null)
			{
				lock (_criteriaXLSLock)
				{
					_criteriaXLS = GetXlsCriteriaInternal();
				}
			}
			return _criteriaXLS;
		}

		public bool ClearCriteriaXLS()
		{
			lock (_criteriaXLS)
			{
				_criteriaXLS = null;
			}
			return true;
		}

		public IEnumerable<OptReportCriteriaGroup> GetCriteriaGroupsForReport(int shopId)
		{
			var critriaGroups = GetCriteriaGroups();
			return critriaGroups.Where(criteriaGroup => criteriaGroup.ShopId == shopId && criteriaGroup.Type != (int)ReportCriteriaType.ReportCriteria);
		}

		public bool ClearCriteriaGroups()
		{
			lock (_criteriaGroups)
			{
				_criteriaGroups = null;
			}
			return true;
		}

		private IEnumerable<OptReportCriteriaGroup> GetCriteriaGroups()
		{
			if (_criteriaGroups == null)
			{
				lock (_criteriaGroupsLock)
				{
					_criteriaGroups = GetCriteriaGroupsInternal();
				}
			}
			return _criteriaGroups;
		}

		private IEnumerable<OptReportCriteriaGroup> GetCriteriaGroupsInternal()
		{
			var criteriaGroups = _context.OptReportCriteriaGroups.Include(x => x.Criteria).ToList();
			return criteriaGroups;
		}

		private CriteriaDictionary GetCriteriaInternal()
		{
			var criteriaGroups = _context.OptReportCriteriaGroups.Where(x => x.Type == (byte)ReportCriteriaType.ReportCriteria).Include(x => x.Criteria).ToList();
			var criteria = new CriteriaDictionary();
			var shops = _context.OptShop.Include(shop => shop.ShopNames).ToList();
			var shopNames = shops.SelectMany(shopN => shopN.ShopNames);

			var shopTypesWithCriteria = new[] { ShopType.WB, ShopType.Ozon, ShopType.MyShop, ShopType.ReadingCity };
			foreach (var shopType in shopTypesWithCriteria)
			{
				var shopNamesForShop = shopNames.Where(x => x.ShopId == (int)shopType).Select(x => x.Name).Distinct().ToList();
				SetCriteriaForShop(criteriaGroups, criteria, (int)shopType, shopNamesForShop);
			}

			return criteria;
		}

		private void SetCriteriaForShop(IEnumerable<OptReportCriteriaGroup> criteriaGroups, CriteriaDictionary criteria, int shopType, List<string> shopNames)
		{
			var shopCriteriaContainer = new KeyValuePair<int, List<KeyValuePair<ReportKind, List<KeyValuePair<string, string>>>>>
				(shopType, new List<KeyValuePair<ReportKind, List<KeyValuePair<string, string>>>>());
			var criteriaForReport = criteriaGroups.Where(x => x.ShopId == shopType);

			foreach (var criteriaItem in criteriaForReport)
			{
				shopCriteriaContainer.Value.Add(new KeyValuePair<ReportKind, List<KeyValuePair<string, string>>>(criteriaItem.ReportKind,
					criteriaItem.Criteria.Select(x => new KeyValuePair<string, string>(x.Address, x.Information)).ToList()));
			}

			//Set criteria for UPD report			
			if (criteria.ContainsKey(shopCriteriaContainer.Key))
			{
				criteria[shopCriteriaContainer.Key].AddRange(shopCriteriaContainer.Value);
			}
			else
			{
				criteria.Add(shopCriteriaContainer.Key, shopCriteriaContainer.Value);
			}
		}


		private Dictionary<ShopType, Dictionary<ReportKind, List<ExcelNavigator>>> GetXlsCriteriaInternal()
		{
			var criteriaXLS = new Dictionary<ShopType, Dictionary<ReportKind,
				List<ExcelNavigator>>>();

			var wb = new KeyValuePair<ShopType, Dictionary<ReportKind,
					List<ExcelNavigator>>>(ShopType.WB, new Dictionary<ReportKind, List<ExcelNavigator>>());

			wb.Value.Add(ReportKind.Report, new List<ExcelNavigator>
				{ new ExcelNavigator(13,0,  "Вайлдберриз") });

			criteriaXLS.Add(wb.Key, wb.Value);

			var readingCity = new KeyValuePair<ShopType, Dictionary<ReportKind,
					List<ExcelNavigator>>>(ShopType.ReadingCity, new Dictionary<ReportKind, List<ExcelNavigator>>());

			readingCity.Value.Add(ReportKind.Report, new List<ExcelNavigator>
				{ new ExcelNavigator(0,0,  "Поставщик"),
				  new ExcelNavigator(0,1,  "ISBN")});

			criteriaXLS.Add(readingCity.Key, readingCity.Value);
			return criteriaXLS;
		}
	}
}
