using ICan.Common.Domain;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ICan.Common.Utils
{
	public static class Util
	{
		public static string GetShortNum(Guid orderNum)
		{
			var list = orderNum.ToString().Split("-").ToList();
			list.Reverse();
			return list.First();
		}

		public static int? GetProductId(Dictionary<int, IdIsbnNameProductModel> productsWithIds, IWBItemWithProductId item, IEnumerable<string> articlesToIgnore)
		{
			if (string.IsNullOrWhiteSpace(item.Barcode) && string.IsNullOrWhiteSpace(item.SupplierArticle))
				throw new UserException($"Не указан isbn и артикул книги");
			//return null;

			var isbn = item.Barcode.ToLower().Trim();
			if (!string.IsNullOrWhiteSpace(isbn) && isbn.StartsWith("12") && isbn.Length == 15)
				isbn = isbn.Substring(2);
			var article = item.SupplierArticle.ToLower().Trim();
			KeyValuePair<int, IdIsbnNameProductModel> product;
			if (articlesToIgnore.Contains(isbn) || articlesToIgnore.Contains(article))
			{
				return null;
			}
			if (isbn.Contains("9785604001905"))
			{
				product = productsWithIds.First(pr => pr.Value.ISBN.Equals("9785604501092"));
			}
			else
			{
				product = productsWithIds.FirstOrDefault(t => string.Equals(isbn, t.Value.ISBN.Trim(), StringComparison.InvariantCultureIgnoreCase) || (!string.IsNullOrWhiteSpace(t.Value.ArticleNumber) && string.Equals(article, t.Value.ArticleNumber.Trim(), StringComparison.InvariantCultureIgnoreCase)));
			}
			if (product.Equals(default(KeyValuePair<int, IdIsbnNameProductModel>)))
				throw new UserException($"Невозможно определить книгу {item.Date} {item.Barcode}  {item.SupplierArticle}");
			return product.Key;
		}

		public static string GetFile(ILogger logger,
			IConfiguration configuration, string settingsName)
		{
			var filePath = configuration[settingsName];
			if (string.IsNullOrWhiteSpace(filePath))
			{
				logger.LogError("Didn't find file setting in config");
				return string.Empty;
			}
			if (!File.Exists(filePath))
			{
				logger.LogError("Didn't find file specified");
				return string.Empty;
			}
			StringBuilder lines = new StringBuilder();

			using (StreamReader sr = new StreamReader(filePath, Encoding.GetEncoding(1251)))
			{
				// Read the stream to a string, and write the string to the console.
				String line = sr.ReadToEnd();
				lines.Append(line);
			}

			return lines.ToString();
		}


		public static string GetContentType(string path)
		{
			var types = GetMimeTypes();
			var ext = Path.GetExtension(path).ToLowerInvariant();
			return types[ext];
		}

	
		public static string GetreviewsDescr(int? reviewsAmount)
		{
			int[] specialNums = new int[4] { 11, 12, 13, 14 };
			if (!reviewsAmount.HasValue)
				return "отзывов";
			var amount = reviewsAmount.Value;
			if (specialNums.Contains(amount) || specialNums.Contains(amount % 100))
				return "отзывов";
			var modResult = amount % 10;
			if (modResult == 0 || modResult >= 5)
				return "отзывов";
			if (modResult == 1)
				return "отзыв";
			return "отзыва";
		}

		public static IEnumerable<SelectListItem> GetAllMonths(int? selected = null)
		{
			var months = Enumerable.Range(1, 12).Select(i =>
			   new SelectListItem
			   {
				   Value = i.ToString(),
				   Text = DateTimeFormatInfo.CurrentInfo.GetMonthName(i),
				   Selected = i == selected
			   });

			return months;
		}	
		
		public static int GetActingPrinting(int printing, int semiproductTypeId)
		{
			return semiproductTypeId != (int)SemiProductType.Cursor ? printing : printing * Const.CursorPrintingCoeff;
		}

		public static int GetOrderBySemiproductType(int semiproductTypeId)
		{
			switch (semiproductTypeId)
			{
				case (int)SemiProductType.Block: return 1;
				case (int)SemiProductType.Covers: return 2;
				case (int)SemiProductType.Stickers: return 3;
				default: return 0;
			}
		}

		private static Dictionary<string, string> GetMimeTypes()
		{
			return new Dictionary<string, string>
			{
				{".mp4", "video/mp4" },
				{".pdf", "application/pdf" },
				{".svg", "svg+xml" },
				{".png", "image/png"},
				{".jpg", "image/jpeg"},
				{".jpeg", "image/jpeg"},
				{".gif", "image/gif"}
			};
		}


	}
}
