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

        public IActionResult Users(string sortOrder, string nameFilter,
            string typeFilter, string addressFilter, 
            int pageSize, int? pageIndex, bool download)
        {
            addressFilter = addressFilter is null ? "" : addressFilter.Trim();
            nameFilter = nameFilter is null ? "" : nameFilter.Trim();

            var userDTOlist = _userService.GetAll();
            var mapper = new MapperConfiguration(cfg => {
                cfg.CreateMap<UserDto, UserViewModel>();
                cfg.CreateMap<AddressDto, AddressViewModel>();
            }).CreateMapper();

            var result = mapper.Map<IEnumerable<UserDto>, List<UserViewModel>>(userDTOlist);


            ViewData["SortOrder"] = sortOrder;
            ViewData["IdSortParam"] = String.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewData["NameSortParam"] = sortOrder == "Name" ? "name_desc" : "Name";
            ViewData["UserTypeSortParam"] = sortOrder == "UserType" ? "usertype_desc" : "UserType";
            ViewData["AddressSortParam"] = sortOrder == "Address" ? "address_desc" : "Address";

            ViewData["NameFilter"] = string.IsNullOrEmpty(nameFilter) ? "" : nameFilter;
            ViewData["UserTypeFilter"] = string.IsNullOrEmpty(typeFilter) ? "" : typeFilter;
            ViewData["AddressFilter"] = string.IsNullOrEmpty(addressFilter) ? "" : addressFilter;

            pageSize = pageSize > 5 ? pageSize : 5;
            pageIndex = pageIndex.HasValue ? pageIndex : 1;

            switch (sortOrder)
            {
                case "id_desc":
                    result.Reverse();
                    break;

                case "Name":
                    result = result.OrderBy(r => r.Name).ToList();
                    break;

                case "name_desc":
                    result = result.OrderByDescending(r => r.Name).ToList();
                    break;

                case "UserType":
                    result = result.OrderBy(r => r.UserType).ToList();
                    break;

                case "usertype_desc":
                    result = result.OrderByDescending(r => r.UserType).ToList();
                    break;

                case "Address":
                    result = result.OrderBy(r => r.Address.ToString()).ToList();
                    break;

                case "address_desc":
                    result = result.OrderByDescending(r => r.Address.ToString()).ToList();
                    break;
            }

            if (nameFilter is not null && nameFilter.Length != 0)
                result = result.FindAll(user => user.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase));

            if (typeFilter is not null && typeFilter.Length != 0)
                result = result.FindAll(user => user.UserType.Contains(typeFilter));

            if (addressFilter is not null && addressFilter.Length != 0)
                result = result.FindAll(user => user.Address.ToString().Contains(addressFilter, StringComparison.OrdinalIgnoreCase));

            var res = _addressService.GetAll();

            ViewData["UserTypes"] = _userService.GetAvailableUserTypes();

            ViewData["AllBuildings"] = res.Select(adr => adr.Building).Distinct();
            ViewData["AllStreets"] = res.Select(adr => adr.Street).Distinct();
            ViewData["AllCities"] = res.Select(adr => adr.City).Distinct();
            ViewData["AllCountries"] = res.Select(adr => adr.Country).Distinct();

            _logger.LogInformation($"Viewing info about all users: page={pageIndex} with pageSize={pageSize}, nameFilter={nameFilter}, " +
                $"typeFilter={typeFilter}, addressFilter={addressFilter} and sortOrder={sortOrder}");

            if (download)
            {
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
                }

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

            return View(PaginatedList<UserViewModel>.CreateList(result.AsQueryable(), pageIndex.Value, pageSize));
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
