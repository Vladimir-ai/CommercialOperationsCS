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
using System.Threading.Tasks;
using App.Domain.BLL;
using System.IO;
using ClosedXML.Excel;

namespace App.Domain.WEB.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;

        private readonly IUserService _userService;
        private readonly IAddressService _addressService;

        public UserController(ILogger<UserController> logger,
            IUserService userService,
            IAddressService addressService)
        {
            _logger = logger;
            _userService = userService;
            _addressService = addressService;
        }

        public IActionResult Users(UsersFilterViewModel filterViewModel, 
            int pageSize = 5, int pageIndex = 1)
        {
            var mapper = new MapperConfiguration(cfg => {
                cfg.CreateMap<UserDto, UserViewModel>();
                cfg.CreateMap<AddressDto, AddressViewModel>();
            }).CreateMapper();
            

            ViewData["SortOrder"] = filterViewModel.SortOrder;
            ViewData["IdSortParam"] = String.IsNullOrEmpty( filterViewModel.SortOrder) ? "id_desc" : "";
            ViewData["NameSortParam"] =  filterViewModel.SortOrder == "Name" ? "name_desc" : "Name";
            ViewData["UserTypeSortParam"] =  filterViewModel.SortOrder == "UserType" ? "usertype_desc" : "UserType";
            ViewData["AddressSortParam"] =  filterViewModel.SortOrder == "Address" ? "address_desc" : "Address";
            ViewData["TotalBoughtValueSortParam"] = filterViewModel.SortOrder == "TotalBoughtValue" ? "total_bought_val_desc" : "TotalBoughtValue"; 
            ViewData["TotalSoldValueSortParam"] = filterViewModel.SortOrder == "TotalSoldValue" ? "total_sold_val_desc" : "TotalSoldValue"; 
            ViewData["TotalBoughtAmountSortParam"] = filterViewModel.SortOrder == "TotalBoughtAmount" ? "total_bought_am_desc" : "TotalBoughtAmount";
            ViewData["TotalSoldAmountSortParam"] = filterViewModel.SortOrder == "TotalSoldAmount" ? "total_sold_am_desc" : "TotalSoldAmount";
            
            ViewData["NameFilter"] = string.IsNullOrEmpty(filterViewModel.NameFilter) ? "" : filterViewModel.NameFilter;
            ViewData["UserTypeFilter"] = string.IsNullOrEmpty( filterViewModel.TypeFilter) ? "" : filterViewModel.TypeFilter;
            ViewData["AddressFilter"] = string.IsNullOrEmpty(filterViewModel.AddressFilter) ? "" : filterViewModel.AddressFilter;

            ViewData["TotalBoughtValueMin"] = filterViewModel.MinBoughtItemValue == 0 ? null : filterViewModel.MinBoughtItemValue;
            ViewData["TotalBoughtValueMax"] = filterViewModel.MaxBoughtItemValue == float.MaxValue ? null : filterViewModel.MaxBoughtItemValue;

            ViewData["TotalSoldValueMin"] = filterViewModel.MinSoldItemValue == 0 ? null :filterViewModel.MinSoldItemValue;
            ViewData["TotalSoldValueMax"] = filterViewModel.MaxSoldItemValue == float.MaxValue ? null : filterViewModel.MaxSoldItemValue;
            
            ViewData["TotalBoughtAmountMin"] = filterViewModel.MinBoughtItemAmount == 0 ? null : filterViewModel.MinBoughtItemAmount;
            ViewData["TotalBoughtAmountMax"] = filterViewModel.MaxBoughtItemAmount == int.MaxValue ? null : filterViewModel.MaxBoughtItemAmount;
            
            ViewData["TotalSoldAmountMin"] = filterViewModel.MinSoldItemAmount == 0 ? null : filterViewModel.MinSoldItemAmount;
            ViewData["TotalSoldAmountMax"] = filterViewModel.MaxSoldItemAmount == int.MaxValue ? null : filterViewModel.MaxSoldItemAmount;
            
            var res = _addressService.GetAll();

            ViewData["UserTypes"] = _userService.GetAvailableUserTypes();

            ViewData["AllBuildings"] = res.Select(adr => adr.Building).Distinct();
            ViewData["AllStreets"] = res.Select(adr => adr.Street).Distinct();
            ViewData["AllCities"] = res.Select(adr => adr.City).Distinct();
            ViewData["AllCountries"] = res.Select(adr => adr.Country).Distinct();
            
            var result = FilterResults(filterViewModel);

            ViewData["TotalValue"] = result.Select(usr => usr.TotalBoughtValue).Sum();
            ViewData["TotalAmount"] = result.Select(usr => usr.TotalBoughtAmount).Sum();
            
            _logger.LogInformation($"Viewing info about all users: page={pageIndex} with pageSize={pageSize}");

            return View(PaginatedList<UserViewModel>.CreateList(result.AsQueryable(), pageIndex, pageSize));
        }

        public IActionResult Download(UsersFilterViewModel filter)
        {
            var result = FilterResults(filter);
            
            using var workbook = new XLWorkbook();

            _logger.LogInformation($"Saving xlsx file for users");

            var worksheet = workbook.Worksheets.Add("Users");
            worksheet.Cell("A1").Value = "Id";
            worksheet.Cell("B1").Value = "User Name";
            worksheet.Cell("C1").Value = "User Country";
            worksheet.Cell("D1").Value = "User City";
            worksheet.Cell("E1").Value = "User Street";
            worksheet.Cell("F1").Value = "User Building";
            worksheet.Cell("G1").Value = "User Type";
            worksheet.Cell("H1").Value = "User Total Bought Amount";
            worksheet.Cell("I1").Value = "User Total Sold Amount";
            worksheet.Cell("J1").Value = "User Total Bought Value";
            worksheet.Cell("K1").Value = "User Total Sold Value";

            worksheet.Cell("M1").Value = "Users Total Bought Amount";
            worksheet.Cell("N1").Value = "Users Total Sold Amount";
            worksheet.Cell("O1").Value = "Users Total Bought Value";
            worksheet.Cell("P1").Value = "Users Total Sold Value";
            worksheet.Cell("R1").Value = "Users Total";
            worksheet.Cell("R2").Value = $"{result.Count}";
            
            int row = 1;
            foreach (var item in result)
            {
                var rowObj = worksheet.Row(++row);
                rowObj.Cell(1).Value = item.Id;
                rowObj.Cell(2).Value = item.Name;
                rowObj.Cell(3).Value = item.Address.Country;
                rowObj.Cell(4).Value = item.Address.City;
                rowObj.Cell(5).Value = item.Address.Street;
                rowObj.Cell(6).Value = item.Address.Building;
                rowObj.Cell(7).Value = item.UserType;
                rowObj.Cell(8).Value = item.TotalBoughtAmount;
                rowObj.Cell(9).Value = item.TotalSoldAmount;
                rowObj.Cell(10).Value = item.TotalBoughtValue;
                rowObj.Cell(11).Value = item.TotalSoldValue;
            }

            worksheet.Cell("M1").FormulaA1 = $"=SUM($H$2:$H${row})";
            worksheet.Cell("N1").FormulaA1 = $"=SUM($I$2:$I${row})";
            worksheet.Cell("O1").FormulaA1 = $"=SUM($J$2:$J${row})";
            worksheet.Cell("P1").FormulaA1 = $"=SUM($K$2:$K${row})";
            
            

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "Users.xlsx",
                Inline = false,
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            using (MemoryStream stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");
            }

        }

        private List<UserViewModel> FilterResults(UsersFilterViewModel filterViewModel)
        {
            var userDtOlist = _userService.GetAll();
            var mapper = new MapperConfiguration(cfg => {
                cfg.CreateMap<UserDto, UserViewModel>();
                cfg.CreateMap<AddressDto, AddressViewModel>();
            }).CreateMapper();

            var result = mapper.Map<IEnumerable<UserDto>, List<UserViewModel>>(userDtOlist);

            result
                .ForEach(user =>
                {
                    user.TotalBoughtAmount = _userService.GetTotalBoughtAmountById(user.Id) ?? 0;
                    user.TotalSoldAmount = _userService.GetTotalSoldAmountById(user.Id) ?? 0;
                    user.TotalBoughtValue = _userService.GetTotalBoughtValueById(user.Id) ?? 0;
                    user.TotalSoldValue = _userService.GetTotalSoldValueById(user.Id) ?? 0;
                });
            
            filterViewModel.ApplyFilter(ref result);
            filterViewModel.FilterByAmounts(ref result);
            filterViewModel.SortUsingOrder(ref result);

            return result;
        }
        
        [HttpPost]
        public IActionResult Users(UserViewModel user, AddressViewModel addressView)
        {
            if (user.Id == 0)
                _logger.LogInformation($"Adding new user");
            else
                _logger.LogInformation($"Updating user with id={user.Id}");

            var mapper = new MapperConfiguration(cfg => {
                cfg.CreateMap<UserViewModel, UserDto> ();
                cfg.CreateMap<AddressViewModel, AddressDto>();
            }).CreateMapper();

            user.Address = addressView;
            
            var result = mapper.Map<UserViewModel, UserDto>(user);

            _addressService.AddOrUpdate(mapper.Map<AddressViewModel, AddressDto>(addressView));
            _userService.AddOrUpdate(result);

            return RedirectPermanent("~/User/Users");
        }

        public IActionResult RemoveUser(long id)
        {
            _logger.LogInformation($"Removing user with id={id}");
            _userService.Delete(id);
            return RedirectPermanent("~/User/Users");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
