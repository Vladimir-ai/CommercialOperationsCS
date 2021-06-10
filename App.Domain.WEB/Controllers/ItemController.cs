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

        public IActionResult Items(ItemFilterViewModel filterViewModel, 
            int pageSize = 5, int pageIndex = 1)
        {
            var categories = _categoryService.GetAll();

            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ItemDto, ItemViewModel>();
                cfg.CreateMap<CategoryDto, CategoryViewModel>();
            }).CreateMapper();
            
            
            ViewData["MinVal"] = filterViewModel.MinVal == 0 ? null : filterViewModel.MinVal;
            ViewData["MaxVal"] = filterViewModel.MaxVal == float.MaxValue ? null : filterViewModel.MaxVal;
            
            ViewData["MinAmount"] = filterViewModel.MinAmount == 0 ? null : filterViewModel.MinAmount;
            ViewData["MaxAmount"] = filterViewModel.MaxAmount == int.MaxValue ? null : filterViewModel.MaxAmount;
            
            ViewData["CatFilter"] = filterViewModel.CatFilter;
            ViewData["NameFilter"] = filterViewModel.NameFilter;

            ViewData["Cats"] = mapper.Map<IEnumerable<CategoryDto>, List<CategoryViewModel>>(categories);

            var sortOrder = filterViewModel.SortOrder;
            ViewData["SortOrder"] = sortOrder;
            ViewData["IdSortParam"] = String.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewData["NameSortParam"] = sortOrder == "Name" ? "name_desc" : "Name";
            ViewData["CatNameSortParam"] = sortOrder == "Category" ? "cat_desc" : "Category";
            ViewData["ValueSortParam"] = sortOrder == "Value" ? "val_desc" : "Value";
            ViewData["AmountSortParam"] = sortOrder == "Amount" ? "amount_desc" : "Amount";

            ViewData["StartDate"] = filterViewModel.StartDate == DateTime.MinValue ? "" : filterViewModel.StartDate.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = filterViewModel.EndDate == DateTime.MaxValue ? "" : filterViewModel.EndDate.ToString("yyyy-MM-dd");

            
            var result = FilterResults(filterViewModel);

            ViewData["TotalValue"] = result.Select(it => it.TotalValue).Sum();
            ViewData["TotalAmount"] = result.Select(it => it.TotalAmount).Sum();
            
            _logger.LogInformation(
                $"Showing info for items: page={pageIndex} with size={pageSize}, category filter=" +
                        $"{String.Join(", ", filterViewModel.CatFilter)} and sortOrder={sortOrder}");

            return View(PaginatedList<ItemViewModel>
                .CreateList(result.AsQueryable(), pageIndex, pageSize));
        }

        public IActionResult Download(ItemFilterViewModel filterViewModel)
        {
            var result = FilterResults(filterViewModel);

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
                it.TotalAmount = _itemService.GetTotalAmountWithinDate(it.Id, 
                    filterViewModel.StartDate, filterViewModel.EndDate);
                
                it.TotalValue = _itemService.GetTotalValueWithinDate(it.Id, 
                    filterViewModel.StartDate, filterViewModel.EndDate);
            });


            filterViewModel.SortUsingOrder(ref result);
            filterViewModel.FilterByValue(ref result);
            filterViewModel.FilterByAmount(ref result);
            
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