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
using System.IO;
using ClosedXML.Excel;

namespace App.Domain.WEB.Controllers
{
    public class OperationController : Controller
    {
        private readonly ILogger<OperationController> _logger;

        private readonly IOperationService _operationService;
        private readonly IItemService _itemService;
        private readonly ICategoryService _categoryService;
        private readonly IUserService _userService;

        public OperationController(ILogger<OperationController> logger,
            IOperationService operationService,
            IItemService itemService,
            ICategoryService categoryService,
            IUserService userService)
        {
            _logger = logger;
            _itemService = itemService;
            _operationService = operationService;
            _userService = userService;
            _categoryService = categoryService;
        }

        public IActionResult Operations(string sortOrder, 
            string[] itemNameFilter, string[] catFilter, 
            string sellUsrNameFilter, string buyUsrNameFilter,
            bool group, int pageSize = 5, int pageIndex = 1,
            int minAmount = 0, int maxAmount = int.MaxValue,
            float minValue = 0, float maxValue = float.MaxValue,
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var res = _operationService.GetAll();

            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<OperationDto, OperationViewModel>();
                cfg.CreateMap<ItemDto, ItemViewModel>();
                cfg.CreateMap<CategoryDto, CategoryViewModel>();
                cfg.CreateMap<UserDto, UserViewModel>().ForMember(dst => dst.Address, src => src.Ignore());
            }).CreateMapper();

            ViewData["AllUsers"] = mapper.Map<IEnumerable<UserDto>, List<UserViewModel>>(_userService.GetAll());
            ViewData["AllCats"] = mapper.Map<IEnumerable<CategoryDto>, List<CategoryViewModel>>(_categoryService.GetAll());
            ViewData["AllItems"] = mapper.Map<IEnumerable<ItemDto>, List<ItemViewModel>>(_itemService.GetAll());
            
            var result = mapper.Map<IEnumerable<OperationDto>, List<OperationViewModel>>(res);

            
            catFilter = catFilter is null || catFilter.Length == 0 ? 
                Array.Empty<string>() : catFilter.Length == 1 ? 
                    catFilter[0].Split(",") : catFilter;
            
            
            itemNameFilter = itemNameFilter is null || itemNameFilter.Length == 0 ? 
                Array.Empty<string>() : itemNameFilter.Length == 1 ?
                    itemNameFilter[0].Split(",") : itemNameFilter;


            ViewData["SortOrder"] = sortOrder;
            ViewData["IdSortParam"] = String.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewData["ItemNameSortParam"] = sortOrder == "ItemName" ? "name_desc" : "ItemName";
            ViewData["ItemCatSortParam"] = sortOrder == "ItemCat" ? "cat_name_desc" : "ItemCat";
            ViewData["ItemCountSortParam"] = sortOrder == "ItemCount" ? "count_desc" : "ItemCount";
            ViewData["ItemValueSortParam"] = sortOrder == "ItemValue" ? "value_desc" : "ItemValue";
            ViewData["BuyUserSortParam"] = sortOrder == "BuyUser" ? "buy_usr_desc" : "BuyUser";
            ViewData["SellUserSortParam"] = sortOrder == "SellUser" ? "buy_usr_desc" : "SellUser";

            ViewData["ItemNameFilter"] = itemNameFilter is null ? Array.Empty<string>() : itemNameFilter;
            ViewData["CatFilter"] = catFilter is null ? Array.Empty<string>() : catFilter;
            ViewData["BuyUserFilter"] = String.IsNullOrEmpty(buyUsrNameFilter) ? "" : buyUsrNameFilter;
            ViewData["SellUserFilter"] = String.IsNullOrEmpty(sellUsrNameFilter) ? "" : sellUsrNameFilter;
            
            switch (sortOrder)
            {
                case "id_desc":
                    result.Reverse();
                    break;

                case "ItemName":
                    result = result.OrderBy(r => r.Item.Name).ToList();
                    break;

                case "name_desc":
                    result = result.OrderByDescending(r => r.Item.Name).ToList();
                    break;

                case "ItemCat":
                    result = result.OrderBy(r => r.Item.Categories[0].Name).ToList();
                    break;

                case "cat_name_desc":
                    result = result.OrderByDescending(r => r.Item.Categories[0].Name).ToList();
                    break;

                case "ItemCount":
                    result = result.OrderBy(r => r.ItemCount).ToList();
                    break;

                case "count_desc":
                    result = result.OrderByDescending(r => r.ItemCount).ToList();
                    break;

                case "ItemValue":
                    result = result.OrderBy(r => r.Value).ToList();
                    break;

                case "value_desc":
                    result = result.OrderByDescending(r => r.Value).ToList();
                    break;

                case "BuyUser":
                    result = result.OrderBy(r => r.SellingUser.Name).ToList();
                    break;

                case "buy_usr_desc":
                    result = result.OrderByDescending(r => r.SellingUser.Name).ToList();
                    break;

                case "SellUser":
                    result = result.OrderBy(r => r.BuyingUser.Name).ToList();
                    break;
            }

            if (itemNameFilter != null && itemNameFilter.Length != 0)
            {
                itemNameFilter = itemNameFilter[0].Split(",");
                result = result.Where(oper => itemNameFilter.Contains(oper.Item.Name)).ToList();
            }

            if (buyUsrNameFilter != null && buyUsrNameFilter.Length != 0)
                result = result.Where(oper => buyUsrNameFilter.Contains(oper.BuyingUser.Name)).ToList();

            if (sellUsrNameFilter != null && sellUsrNameFilter.Length != 0)
                result = result.Where(oper => sellUsrNameFilter.Contains(oper.SellingUser.Name)).ToList();

            if (catFilter != null && catFilter.Length != 0)
            {
                catFilter = catFilter[0].Split(",");
                result = result.Where(oper => oper.Item.Categories.Select(cat => cat.Name).Intersect(catFilter).Any()).ToList();
            }

            _logger.LogInformation($"Viewing info about all operations: pageIndex={pageIndex} with pageSize={pageSize}" +
                $", sellUserNameFilter={sellUsrNameFilter}, buyUserNameFilter={buyUsrNameFilter}, " +
                $"itemNameFilter={String.Join(", ", itemNameFilter)}, categoryFilter={String.Join(", ", catFilter)} and sortOrder={sortOrder}");


            if (download)
            {
                using var workbook = new XLWorkbook();

                _logger.LogInformation($"Saving xlsx file for operations");

                var worksheet = workbook.Worksheets.Add("Operations");
                worksheet.Cell("A1").Value = "Id";
                worksheet.Cell("B1").Value = "Item Name";
                worksheet.Cell("C1").Value = "Item Categories";
                worksheet.Cell("D1").Value = "Item Count";
                worksheet.Cell("E1").Value = "Operation Value";
                worksheet.Cell("F1").Value = "Selling User Name";
                worksheet.Cell("G1").Value = "Selling User Type";
                worksheet.Cell("H1").Value = "Buying User Name";
                worksheet.Cell("I1").Value = "Buying User Type";

                int row = 1;
                foreach (var operation in result)
                {
                    var rowObj = worksheet.Row(++row);
                    rowObj.Cell(1).Value = operation.Id;
                    rowObj.Cell(2).Value = operation.Item.Name;
                    rowObj.Cell(3).Value = String.Join(",", operation.Item.Categories.Select(cat => cat.Name).ToArray());
                    rowObj.Cell(4).Value = operation.ItemCount;
                    rowObj.Cell(5).Value = operation.Value;
                    rowObj.Cell(6).Value = operation.SellingUser.Name;
                    rowObj.Cell(7).Value = operation.SellingUser.UserType;
                    rowObj.Cell(8).Value = operation.BuyingUser.Name;
                    rowObj.Cell(9).Value = operation.BuyingUser.UserType;
                }

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = "Operations.xlsx",
                    Inline = false,
                };
                Response.Headers.Add("Content-Disposition", cd.ToString());
                using (MemoryStream stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Operations.xlsx");
                }
            }


            return View(PaginatedList<OperationViewModel>.CreateList(result.AsQueryable(), pageIndex.Value, pageSize));
        }


        private List<OperationViewModel> FilterResults(OperationFilterViewModel filter, int pageSize = 5, int pageIndex = 1)
        {
            
        }
        
        [HttpPost]
        public IActionResult Operations(OperationViewModel operation, long buyingUser, long sellingUser, long item)
        {
            if (operation.Id == 0)
                _logger.LogInformation($"Adding new operation");
            else
                _logger.LogInformation($"Updating operation with id={operation.Id}");

            if (buyingUser == sellingUser)
                return RedirectPermanent("~/Operation/Operations");

            operation.BuyingUser = new UserViewModel { Id = buyingUser };
            operation.SellingUser = new UserViewModel { Id = sellingUser };
            operation.Item = new ItemViewModel { Id = item };

            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserViewModel, UserDto>();
                cfg.CreateMap<ItemViewModel, ItemDto>();
                cfg.CreateMap<OperationViewModel, OperationDto>();
            }).CreateMapper();

            _operationService.AddOrUpdate(mapper.Map<OperationViewModel, OperationDto>(operation));
            return RedirectPermanent("~/Operation/Operations");
        }

        public IActionResult RemoveOperation(long id)
        {
            _logger.LogInformation($"Removing operation with id={id}");
            _operationService.Delete(id);
            return RedirectPermanent("~/Operation/Operations");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
