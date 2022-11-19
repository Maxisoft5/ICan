using AutoMapper;
using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models;
using ICan.Common.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using ICan.Common.Models.Opt;
using ICan.Common;
using System.Linq;

namespace ICan.Controllers
{
    [Authorize(Roles = "Admin,Operator")]
    [ServiceFilter(typeof(SetRoleAttribute))]
    public class ShopController : BaseController
    {
        private readonly ShopManager _shopManager;
        private string FilesFolder;
        public ShopController(IMapper mapper,
                UserManager<ApplicationUser> userManager, ILogger<BaseController> logger,
                IConfiguration configuration,
                ShopManager shopManager)
                : base(mapper, userManager, logger, configuration)
        {
            _shopManager = shopManager;
            FilesFolder = configuration["Settings:FilesFolder"];
        }

        public async Task<IActionResult> Index()
        {
            var shops = await _shopManager.GetShops();
            return View(shops);
        }

        /// <summary>
        /// get shops list as a dictionary for bootstrap table filter
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<IActionResult> Dict()
        {
            var shops = await _shopManager.Dict();
            return Ok(shops);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,ShopUrl,INN,IsMarketPlace,NonResident,Postponement,ScanFileName,IgnoreInWarehouseCalc")] ShopModel model,
            IFormFile scanFile)
        {
            if (ModelState.IsValid)
            {
                await _shopManager.Create(model, scanFile);
                TempData["StatusMessage"] = Const.SuccessMessages.Saved;
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var shop = await _shopManager.GetShop(id);
                return View(shop);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("ShopId,Name,Consignee,Enabled,ShopUrl,IsMarketPlace,NonResident,Postponement,ScanFileName," +
            "MimeType,IgnoreInWarehouseCalc")] OptShop optShop,
            IFormFile scanFile)
        {
            if (id != optShop.ShopId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _shopManager.Edit(optShop, scanFile);
                    TempData["StatusMessage"] = Const.SuccessMessages.Saved;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
                    _logger.LogError(ex, "[Магазин] ошибка при сохранении магазина");

                    if (!_shopManager.OptShopExists(optShop.ShopId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
                    _logger.LogError(ex, "[Магазин] ошибка при сохранении магазина");
                }
                return RedirectToAction(nameof(Index));
            }
            return View(optShop);
        }

        // POST: OptShops/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _shopManager.Delete(id);
                TempData["StatusMessage"] = "Магазин удалён";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Невозможно удалить магазин";
                _logger.LogError(ex, string.Format(Const.ErrorMessages.CantDelete, id));
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("AddShopName")]
        public async Task<IActionResult> AddShopName(IFormCollection collection)
        {
            try
            {
                int.TryParse(collection["shopId"].ToString(), out var shopId);
                var shopName = collection["Name"].ToString();
                var inn = collection["inn"].ToString();

                bool enabled = string.Equals(collection["Enabled"].ToString(), "on", StringComparison.OrdinalIgnoreCase);
                OptShopName raw = new OptShopName
                {
                    Name = shopName,
                    ShopId = shopId,
                    Inn = inn,
                    Enabled = true
                };
                await _shopManager.AddShopName(raw);
                TempData["StatusMessage"] = "Успешно сохранено";
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
                return BadRequest();
            }
        }

        [HttpPost, ActionName("DeleteShopName")]
        public async Task<IActionResult> DeleteShopName(int id)
        {
            try
            {
                await _shopManager.DeleteShopName(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
                return BadRequest();
            }
        }

        public IActionResult Preview(string url, string mimeType)
        {
            return PhysicalFile(Path.Combine(FilesFolder, url), mimeType);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteContractFile(int shopId)
        {
            await _shopManager.DeleteContractFile(shopId);
            return Ok();
        }

        public async Task<IActionResult> GetAvailableShops(string existingIds)
        {
            var existingList = existingIds?.Split(",").Select(item => int.Parse(item)) ?? Enumerable.Empty<int>();
            var shops = await _shopManager.GetShops();
            var exceptExistings = shops.Where(x => !existingList.Contains(x.ShopId));
            return Ok(exceptExistings.Select(x => new { Text = x.Name, Value = x.ShopId }));
        }

        public async Task<IActionResult> UnbindShop(int shopId, string userId)
        {
            await _shopManager.UnbindShop(shopId, userId);
            return Ok();
        }
    }
}
