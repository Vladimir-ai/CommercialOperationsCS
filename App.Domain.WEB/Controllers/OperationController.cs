using App.Domain.BLL.DTO;
using App.Domain.BLL.Interfaces;
using App.Domain.WEB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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

        public IActionResult Operations(OperationFilterViewModel filterViewModel,
            int pageSize = 5, int pageIndex = 1)
        {
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
            //ViewData["AllGroups"] = new[] {"Buying User", "Selling User", "Item"};
            
            
            ViewData["SortOrder"] = filterViewModel.SortOrder;
            ViewData["IdSortParam"] = String.IsNullOrEmpty(filterViewModel.SortOrder) ? "id_desc" : "";
            ViewData["ItemNameSortParam"] = filterViewModel.SortOrder == "ItemName" ? "name_desc" : "ItemName";
            ViewData["ItemCatSortParam"] = filterViewModel.SortOrder == "ItemCat" ? "cat_name_desc" : "ItemCat";
            ViewData["ItemCountSortParam"] = filterViewModel.SortOrder == "ItemCount" ? "count_desc" : "ItemCount";
            ViewData["ItemValueSortParam"] = filterViewModel.SortOrder == "ItemValue" ? "value_desc" : "ItemValue";
            ViewData["BuyUserSortParam"] = filterViewModel.SortOrder == "BuyUser" ? "buy_usr_desc" : "BuyUser";
            ViewData["SellUserSortParam"] = filterViewModel.SortOrder == "SellUser" ? "sell_usr_desc" : "SellUser";
            ViewData["DateSortParam"] = filterViewModel.SortOrder == "Date" ? "date_desc" : "Date";

            
            ViewData["ItemNameFilter"] = filterViewModel.ItemNameFilter;
            ViewData["CatFilter"] = filterViewModel.CatFilter;
            ViewData["BuyUserFilter"] = filterViewModel.BuyUsrNameFilter;
            ViewData["SellUserFilter"] = filterViewModel.SellUsrNameFilter;

            ViewData["MinVal"] = filterViewModel.MinValue == 0 ? null : filterViewModel.MinValue;
            ViewData["MaxVal"] = filterViewModel.MaxValue == float.MaxValue ? null : filterViewModel.MaxValue;

            ViewData["MinAmount"] = filterViewModel.MinAmount == 0 ? null : filterViewModel.MinAmount;
            ViewData["MaxAmount"] = filterViewModel.MaxAmount == int.MaxValue ? null : filterViewModel.MaxAmount;

            ViewData["StartDate"] = filterViewModel.StartDate == DateTime.MinValue ? "" : filterViewModel.StartDate.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = filterViewModel.EndDate == DateTime.MaxValue ? "" : filterViewModel.EndDate.ToString("yyyy-MM-dd");

            //ViewData["CurrGroup"] = filterViewModel.Group;
            
            var result = FilterResults(filterViewModel);

            ViewData["TotalAmount"] = result.Select(it => it.ItemCount).Sum();
            ViewData["TotalValue"] = result.Select(it => it.Value).Sum();
            
            _logger.LogInformation($"Viewing info about all operations: pageIndex={pageIndex} with pageSize={pageSize}");
            
            return View(PaginatedList<OperationViewModel>.CreateList(result.AsQueryable(), pageIndex, pageSize));
        }

        public IActionResult Download(OperationFilterViewModel filter)//TODO
        {
            var result = FilterResults(filter);
            
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
                worksheet.Cell("J1").Value = "Operation Date";

                worksheet.Cell("L1").Value = "Total Amount";
                worksheet.Cell("M1").Value = "Total Value";

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
                    rowObj.Cell(10).Value = operation.SellingDate.ToString("d", CultureInfo.CreateSpecificCulture("ru-RU"));
                }

                worksheet.Cell("L2").FormulaA1 = $"=SUM($D$2:$D${row})";
                worksheet.Cell("M2").FormulaA1 = $"=SUM($E$2:$E${row})";
                
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

        private List<OperationViewModel> FilterResults(OperationFilterViewModel filter)
        {
            var res = _operationService.GetAll();

            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<OperationDto, OperationViewModel>();
                cfg.CreateMap<ItemDto, ItemViewModel>();
                cfg.CreateMap<CategoryDto, CategoryViewModel>();
                cfg.CreateMap<UserDto, UserViewModel>().ForMember(dst => dst.Address, src => src.Ignore());
            }).CreateMapper();

            var result = mapper.Map<IEnumerable<OperationDto>, List<OperationViewModel>>(res);
            
            filter.SortByAmount(ref result);
            filter.SortByCatUsr(ref result);
            filter.SortUsingOrder(ref result);
            
            
            
            return result;
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
