using Amazon.S3;
using Amazon.S3.Model;
using AutoMapper;
using ICan.Common.Domain;
using ICan.Common.Models.Opt;
using ICan.Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class AdminManager : BaseManager
	{
		public AdminManager(IMapper mapper, ApplicationDbContext context, ILogger<BaseManager> logger) : base(mapper, context, logger)
		{
		}

		public bool Migrate()
		{
			try
			{
				_context.Database.Migrate();
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обновлении цен");
				return false;
			}
		}

		public List<ProductpriceModel> GetPrices(int? productId)
		{
			var now = DateTime.Now;
			return _context
				.OptProductprice
				.Include(t => t.Product)
				.Where(q => q.ProductId == productId)
				.Where(q => (q.DateEnd.HasValue && q.DateStart.HasValue &&
					now >= q.DateStart.Value &&
					now < q.DateEnd.Value)
					|| (!q.DateEnd.HasValue && q.DateStart.HasValue && now >= q.DateStart)
					|| (!q.DateEnd.HasValue && !q.DateStart.HasValue))
				.Select(t => new ProductpriceModel(t)).ToList();
		}

		public ProductpriceModel Get(int productPriceId)
		{
			OptProductprice raw = GetInternal(productPriceId);
			var model = _mapper.Map<ProductpriceModel>(raw);
			return model;
		}


		public async Task RemoveAsync(int id)
		{
			var raw = GetInternal(id);
			_context.OptProductprice.Remove(raw);
			await _context.SaveChangesAsync();
		}

		public async Task<byte[]> ParseAndClean(IFormFile file)
		{
			var data = await ParseAmazonFile(file);
			byte[] bytes = GetFile(data);
			return bytes;
			//createnew
		}

		private byte[] GetFile(List<SheetData> data)
		{
			var allWorksheets = data.ToArray<SheetData>();
			using (ExcelPackage objExcelPackage = new ExcelPackage())
			{
				var stop = allWorksheets[0] as StopSheetData;
				PrintStopSheet(objExcelPackage, stop);
				for (int i = 1; i < allWorksheets.Count(); i++)
				{
					var sheet = (allWorksheets[i]) as ContentSheetData;
					var objWorksheet = objExcelPackage.Workbook.Worksheets.Add(sheet.Title);
					var currentRow = 1;
					objWorksheet.Cells[$"A{currentRow}"].Value = "AMAZON SEARCH";
					
					objWorksheet.Cells[$"C{currentRow}"].Value = "SEARCH VOUME"; 
					objWorksheet.Cells[$"D{currentRow}"].Value = "3M AVG";  
					objWorksheet.Cells[$"E{currentRow}"].Value = "3M AVG";  
					objWorksheet.Cells[$"F{currentRow}"].Value = "DEPTH";  
					objWorksheet.Cells[$"G{currentRow}"].Value = "RESULTS";  
					objWorksheet.Cells[$"H{currentRow}"].Value = "SPONSORED AD";
					objWorksheet.Cells[$"I{currentRow}"].Value = "PAGE 1 REVIEW";
					objWorksheet.Cells[$"J{currentRow}"].Value = "APPEARANCE";
					objWorksheet.Cells[$"K{currentRow}"].Value = "LAST SEEN";

					currentRow++;
				
					foreach (var dataItem in sheet.Items)
					{
						objWorksheet.Cells[$"A{currentRow}"].Value = dataItem.CellA;			
						objWorksheet.Cells[$"B{currentRow}"].Value = dataItem.CellB;
						objWorksheet.Cells[$"C{currentRow}"].Value = dataItem.CellC;
						objWorksheet.Cells[$"D{currentRow}"].Value = dataItem.CellD;
						objWorksheet.Cells[$"E{currentRow}"].Value = dataItem.CellE;
						objWorksheet.Cells[$"F{currentRow}"].Value = dataItem.CellF;
						objWorksheet.Cells[$"G{currentRow}"].Value = dataItem.CellG;
						objWorksheet.Cells[$"H{currentRow}"].Value = dataItem.CellH;
						objWorksheet.Cells[$"I{currentRow}"].Value = dataItem.CellI;
						objWorksheet.Cells[$"J{currentRow}"].Value = dataItem.CellJ;
						objWorksheet.Cells[$"K{currentRow}"].Value = dataItem.CellK;
						objWorksheet.Cells[$"L{currentRow}"].Value = dataItem.CellL;
						currentRow++;
					}
					objWorksheet.Column(1).Width = 35;
					objWorksheet.Cells["A1:L"+sheet.Items.Count()].AutoFilter = true;
				}
				return objExcelPackage.GetAsByteArray();
			}
		}

		private void PrintStopSheet(ExcelPackage objExcelPackage, StopSheetData
			sheet)
		{
			var objWorksheet = objExcelPackage.Workbook.Worksheets.Add(sheet.Title);
			int i = 1;
			foreach (var word in sheet.Words)
			{
				objWorksheet.Cells[$"A{i}"].Value = word;
				i++;
			}
		}

		public class SheetData
		{
			public string Title { get; set; }
		}

		public class ContentSheetData : SheetData
		{
			public IEnumerable<AmazonData> Items;
		}

		public class StopSheetData : SheetData
		{
			public IEnumerable<string> Words;
		}

		public class AmazonData
		{
			public string CellA { get; set; }
			public string CellB { get; set; }
			public decimal? CellC { get; set; }
			public decimal? CellD { get; set; }
			public decimal? CellE { get; set; }
			public decimal? CellF { get; set; }
			public decimal? CellG { get; set; }
			public string CellH { get; set; }
			public decimal? CellI { get; set; }
			public string CellJ { get; set; }
			public string CellK { get; set; }
			public string CellL { get; set; }
		}

		private async Task<List<SheetData>> ParseAmazonFile(IFormFile file)
		{
			var list = new List<SheetData>();
			using (var memoryStream = new MemoryStream())
			{
				await file.CopyToAsync(memoryStream).ConfigureAwait(false);

				using (var package = new ExcelPackage(memoryStream))
				{
					var stopSheet = package.Workbook.Worksheets[0];
					var stopContent = ReadStopSheet(stopSheet);
					list.Add(stopContent);

					for (int i = 1; i < package.Workbook.Worksheets.Count; i++)
					{
						var worksheet = package.Workbook.Worksheets[i];
						list.Add(ReadSheet(worksheet, stopContent));
					}
				}
			}
			return list;
		}

		private StopSheetData ReadStopSheet(ExcelWorksheet worksheet)
		{
			var sheetData = new StopSheetData { Title = worksheet.Name };
			var list = new List<string>();
			for (var i = 1; i <= worksheet.Dimension.Rows; i++)
			{
				var word = worksheet.Cells[i, 1].Value?.ToString();
				if (!string.IsNullOrWhiteSpace(word))
					list.Add(word);
			}
			sheetData.Words = list;
			return sheetData;
		}

		private SheetData ReadSheet(ExcelWorksheet worksheet, StopSheetData stopContent)
		{
			var sheetData = new ContentSheetData { Title = worksheet.Name };
			var list = new List<AmazonData>();
			for (var index = 1; index <= worksheet.Dimension.Rows; index++)
			{
				var a = worksheet.Cells[$"A{index}"].Value?.ToString();
				var b = worksheet.Cells[$"B{index}"].Value?.ToString();
				var c = worksheet.Cells[$"C{index}"].Value?.ToString();
				var d = worksheet.Cells[$"D{index}"].Value?.ToString();
				var e = worksheet.Cells[$"E{index}"].Value?.ToString();
				var f = worksheet.Cells[$"F{index}"].Value?.ToString();
				var g = worksheet.Cells[$"G{index}"].Value?.ToString();
				var h = worksheet.Cells[$"H{index}"].Value?.ToString();
				var i = worksheet.Cells[$"I{index}"].Value?.ToString();
				var j = worksheet.Cells[$"J{index}"].Value?.ToString();
				var k = worksheet.Cells[$"K{index}"].Value?.ToString();
				var l = worksheet.Cells[$"L{index}"].Value?.ToString();
				var data = new AmazonData
				{
					CellA = CleanName(a),
					CellB = b,
					CellC = ParseAndReplaceNum(c),
					CellD = ParseAndReplaceNum(d),
					CellE = ParseAndReplaceNum(e),
					CellF = ParseAndReplaceNum(f),
					CellG = ParseAndReplaceNum(g),
					CellH = h,
					CellI = ParseAndReplaceNum(i),
					CellJ = j,
					CellK = k,
					CellL = l,
				};
				if (CanAdd(stopContent, list, data))
					list.Add(data);
			}

			sheetData.Items = list;
			return sheetData;
		}

		private string CleanName(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return string.Empty;
			return value.Replace("P1", "").Replace("\n", "").Trim().ToLower();
		}

		private decimal? ParseAndReplaceNum(string value)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(value))
					return null;
				if (value.Contains("--"))
					return null;
				if (value.Contains("<") && value.Contains("100"))
					return 100;
				return decimal.Parse(value.Replace(",", "").Replace(" ", ""));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "");
				throw;
			}
			// throw new NotImplementedException();
		}

		private bool CanAdd(StopSheetData stopContent, IEnumerable<AmazonData> list, AmazonData data)
		{
			var words = data.CellA.Split(" ");

			foreach (var stopWord in stopContent.Words)
			{ 
				if (words.Contains(stopWord))
					return false;
			}
	
			foreach (var existing in list)
			{
				var existingWords = existing.CellA.Split(" ");
				if (words.Count() != existingWords.Count())
					continue;
				var equals = existingWords.All(existingWord => words.Contains(existingWord));
				if (equals)
					return false;
				equals = words.All(word => existingWords.Contains(word));
				if (equals)
					return false;
			}
			return true;
		}

		private OptProductprice GetInternal(int productPriceId)
		{
			return _context
						   .OptProductprice
						   .FirstOrDefault(q => q.ProductPriceId == productPriceId);
		}

		public async Task AddPrice(int id, double price)
		{
			var date = DateTime.Now.AddSeconds(-1);
			var oldPrice =
				_context.OptProductprice.FirstOrDefault(t => t.ProductId == id &&
				(t.DateEnd == null && t.DateStart <= date));
			if (oldPrice != null)
				oldPrice.DateEnd = date;

			var newPrice = new OptProductprice
			{
				Price = price,
				ProductId = id,
				DateStart = date
			};
			await _context.AddAsync(newPrice);
			await _context.SaveChangesAsync();
		}
	}
}
