using App.Domain.BLL.DTO;
using App.Domain.BLL.Interfaces;
using App.Domain.WEB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoMapper;
using ClosedXML.Excel;
using System.IO;

namespace App.Domain.WEB.Controllers
{
    public class ItemController : Controller
    {
        private readonly ILogger<ItemController> _logger;

        private readonly IItemService _itemService;
        private readonly ICategoryService _categoryService;

        public ItemController(ILogger<ItemController> logger,
            IItemService itemService,
            ICategoryService categoryService)
        {
            _logger = logger;
            _itemService = itemService;
            _categoryService = categoryService;
        }

        public IActionResult Items(string sortOrder, string nameFilter,
            string[] catFilter, int pageSize, int? pageIndex,
            float minVal = 0, float maxVal = float.MaxValue,
            int minAmount = 0, int maxAmount = int.MaxValue)
        {
            
            var categories = _categoryService.GetAll();

            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ItemDto, ItemViewModel>();
                cfg.CreateMap<CategoryDto, CategoryViewModel>();
            }).CreateMapper();

            nameFilter = nameFilter is null ? "" : nameFilter.Trim();
            pageSize = pageSize > 5 ? pageSize : 5;
            pageIndex = pageIndex.HasValue ? pageIndex : 1;
            
            catFilter = catFilter is null || catFilter.Length == 0 ? Array.Empty<string>() :
                catFilter.Length == 1 ? catFilter[0].Split(",") : catFilter;

            ViewData["minVal"] = minVal == 0 ? null : minVal;
            ViewData["maxVal"] = maxVal == float.MaxValue ? null : maxVal;
            
            ViewData["minAmount"] = minAmount == 0 ? null : minAmount;
            ViewData["maxAmount"] = maxAmount == int.MaxValue ? null : minAmount ;
            
            ViewData["CatFilter"] = catFilter;
            ViewData["NameFilter"] = string.IsNullOrEmpty(nameFilter) ? "" : nameFilter;

            ViewData["Cats"] = mapper.Map<IEnumerable<CategoryDto>, List<CategoryViewModel>>(categories);

            ViewData["SortOrder"] = sortOrder;
            ViewData["IdSortParam"] = String.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewData["NameSortParam"] = sortOrder == "Name" ? "name_desc" : "Name";
            ViewData["CatNameSortParam"] = sortOrder == "Category" ? "cat_desc" : "Category";
            ViewData["ValueSortParam"] = sortOrder == "Value" ? "val_desc" : "Value";
            ViewData["AmountSortParam"] = sortOrder == "Amount" ? "amount_desc" : "Amount";

            var result = FilterResults(sortOrder, 
                nameFilter, catFilter, minVal, 
                maxVal, minAmount, maxAmount);

            _logger.LogInformation(
                $"Showing info for items: page={pageIndex} with size={pageSize}, category filter={String.Join(", ", catFilter)} and sortOrder={sortOrder}");

            return View(PaginatedList<ItemViewModel>
                .CreateList(result.AsQueryable(), pageIndex.Value, pageSize));
        }

        public IActionResult Download(string sortOrder,
            string nameFilter, string[] catFilter,
            float minVal = 0, float maxVal = float.MaxValue,
            int minAmount = 0, int maxAmount = int.MaxValue)
        {
            var result = FilterResults(sortOrder,
                nameFilter, catFilter, minVal, maxVal, minAmount, maxAmount);

            using (var workbook = new XLWorkbook())
            {
                _logger.LogInformation($"Saving xlsx file for items");

                var worksheet = workbook.Worksheets.Add("Items");
                worksheet.Cell("A1").Value = "Id";
                worksheet.Cell("B1").Value = "Item Name";
                worksheet.Cell("C1").Value = "Item Categories";
                worksheet.Cell("D1").Value = "Total Item Amount";
                worksheet.Cell("E1").Value = "Total Item Value";

                worksheet.Cell("G1").Value = "Total Amount";
                worksheet.Cell("H1").Value = "Total Value";
                    
                int row = 1;
                foreach (var item in result)
                {
                    var rowObj = worksheet.Row(++row);
                    rowObj.Cell(1).Value = item.Id;
                    rowObj.Cell(2).Value = item.Name;
                    rowObj.Cell(3).Value = String.Join(",",
                        item.Categories.Select(cat => cat.Name)
                            .ToArray());

                    rowObj.Cell(4).Value = item.TotalAmount;
                    rowObj.Cell(5).Value = item.TotalValue;
                    
                }

                worksheet.Cell("G2").FormulaA1 = $"=SUM($D$2:$D${row})";
                worksheet.Cell("H2").FormulaA1 = $"=SUM($E$2:$E${row})";

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = "Items.xlsx",
                    Inline = false,
                };
                Response.Headers.Add("Content-Disposition", cd.ToString());

                using (MemoryStream stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Items.xlsx");
                }
            }
        }

        private List<ItemViewModel> FilterResults(ItemFilterViewModel filterViewModel)
        {
            nameFilter = nameFilter is null ? "" : nameFilter.Trim();
            catFilter = catFilter is null || catFilter.Length == 0 ? Array.Empty<string>() :
                catFilter.Length == 1 ? catFilter[0].Split(",") : catFilter;

            var itemList = _itemService.GetAll();

            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ItemDto, ItemViewModel>();
                cfg.CreateMap<CategoryDto, CategoryViewModel>();
            }).CreateMapper();

            var result = mapper
                .Map<IEnumerable<ItemDto>, List<ItemViewModel>>(itemList);

            result.ForEach(it =>
            {
                it.TotalAmount = _itemService.GetTotalAmount(it.Id);
                it.TotalValue = _itemService.GetTotalValue(it.Id);
            });
            
            result = sortOrder switch 
            {
                "id_desc" => result.AsQueryable().Reverse().ToList(), 
                "Name" => result.OrderBy(r => r.Name).ToList(),
                "name_desc" => result.OrderByDescending(r => r.Name).ToList(),
                "Category" => result.OrderBy(r => r.Categories[0].Name).ToList(),
                "cat_desc" => result.OrderByDescending(r  => r.Categories[0].Name).ToList(),
                "Amount" => result.OrderBy(r => r.TotalAmount).ToList(),
                "amount_desc" => result.OrderByDescending(r => r.TotalAmount).ToList(),
                "Value" => result.OrderBy(r => r.TotalValue).ToList(),
                "val_desc" => result.OrderByDescending(r => r.TotalValue).ToList(),
                _ => result
            };

            if (!nameFilter.Equals(""))
                result = result.FindAll(item => item.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase));

            if (catFilter.Length != 0)
            {
                result = result.Where(item => item.Categories
                        .Select(cat => cat.Name)
                        .Intersect(catFilter).Any())
                        .ToList();
            }

            result = result
                .Where(it => it.TotalAmount <= maxAmount && it.TotalAmount >= minAmount)
                .ToList();

            result = result
                .Where(it => it.TotalValue <= maxVal && it.TotalValue >= minVal)
                .ToList();
            
            return result;
        }

        [HttpPost]
        public IActionResult Items(ItemViewModel item, string[] categories)
        {
            if (item.Id == 0)
                _logger.LogInformation("Adding new item");
            else
                _logger.LogInformation($"Updating item with id={item.Id}");

            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ItemViewModel, ItemDto>();
                cfg.CreateMap<CategoryViewModel, CategoryDto>();
            }).CreateMapper();

            var itemDto = mapper.Map<ItemViewModel, ItemDto>(item);

            itemDto.Categories = _categoryService.FromStringListIgnoreItems(categories.ToList());
            _itemService.AddOrUpdate(itemDto);

            _logger.LogInformation($"item with id {item.Id} has changed");

            return RedirectPermanent("~/Item/Items");
        }

        public IActionResult RemoveItem(long id)
        {
            _logger.LogInformation($"Removing item with id={id}");
            _itemService.Delete(id);
            return RedirectPermanent("~/Item/Items");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}