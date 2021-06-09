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

namespace App.Domain.WEB.Controllers
{
    public class IndexController : Controller
    {
        private readonly ILogger<IndexController> _logger;
        private readonly IUserService _userService;
        private readonly IItemService _itemService;
        private readonly ICategoryService _categoryService;
        private readonly IOperationService _operationService;
        private readonly IAddressService _addressService;

        public IndexController(ILogger<IndexController> logger,
            IUserService userService,
            IItemService itemService,
            ICategoryService categoryService,
            IOperationService operationService,
            IAddressService addressService)
        {
            _logger = logger;
            _userService = userService;
            _itemService = itemService;
            _categoryService = categoryService;
            _operationService = operationService;
            _addressService = addressService;
        }

        public IActionResult Index()
        {
            var result = new List<string>();

            result.Add($"User Amount: {_userService.GetAll().Count()}");
            result.Add($"Item Category Amount: {_categoryService.GetAll().Count()}");
            result.Add($"Item Amount: {_itemService.GetAll().Count()}");
            result.Add($"Operation Amount: {_operationService.GetAll().Count()}");

            var operations = _operationService.GetAll();

            if (!operations.Any())
                return View(result);

            var theMostSellingItemByPrice = operations
                .GroupBy(op => op.Item.Id)
                .OrderByDescending(gr => gr.Sum(item => item.Value))
                .FirstOrDefault()!.Select(igr => igr.Item.Name).FirstOrDefault();
            
            var theMostSellingItemByAmount = operations
                .GroupBy(op => op.Item.Id)
                .OrderByDescending(gr => gr.Sum(item => item.ItemCount))
                .FirstOrDefault()!.Select(igr => igr.Item.Name).FirstOrDefault();

            var theMostBuyingUser = operations
                .GroupBy(op => op.BuyingUser.Id)
                .OrderByDescending(gr => gr.Sum(item => item.Value))
                .FirstOrDefault()!.Select(igr => igr.BuyingUser.Name).FirstOrDefault();
            
            var theMostSellingUser = operations
                .GroupBy(op => op.SellingUser.Id)
                .OrderByDescending(gr => gr.Sum(item => item.Value))
                .FirstOrDefault()!.Select(igr => igr.SellingUser.Name).FirstOrDefault();

            result.Add($"The Most Selling Item (By Price) {theMostSellingItemByPrice}");
            result.Add($"The Most Selling Item (By Amount) {theMostSellingItemByAmount}");
            result.Add($"User, Who Bought The Most Items (By Price) {theMostBuyingUser}");
            result.Add($"User, Who Sold The Most Items (By Price) {theMostSellingUser}");

            return View(result);
        }

        public IActionResult ItemStats(int id)
        {

            _logger.LogInformation($"Showing statst for item with id={id}");
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserDto, UserViewModel>();
                cfg.CreateMap<ItemDto, ItemViewModel>();
                cfg.CreateMap<CategoryDto, CategoryViewModel>();
                cfg.CreateMap<AddressDto, AddressViewModel>();
                cfg.CreateMap<OperationDto, OperationViewModel>();
            }).CreateMapper();

            var item = _itemService.Find(id);
            
            if (item is null)
                return Redirect(Request.Headers["Referer"].ToString());

            var itemStats = new ItemStatsViewModel { Item = mapper.Map<ItemDto, ItemViewModel>(item) };
            
            var opertations = _operationService.GetAll().Where(op => op.Item.Id == item.Id);
          
            itemStats.Item.TotalAmount = _itemService.GetTotalAmount(itemStats.Item.Id);
            itemStats.Item.TotalValue = _itemService.GetTotalValue(itemStats.Item.Id);

            if (!opertations.Any())
            {
                itemStats.MostPopularCityToSell = mapper.Map<AddressDto, AddressViewModel>(opertations.GroupBy(op =>
                        _addressService.Find(op.SellingUser.BuildingId))
                            .OrderByDescending(group => group.Count())
                            .Select(grp => grp.Key).First());
                
                itemStats.MostPopularCityToBuy = mapper.Map<AddressDto, AddressViewModel>(opertations.GroupBy(op =>
                        _addressService.Find(op.BuyingUser.BuildingId))
                            .OrderByDescending(group => group.Count())
                            .Select(grp => grp.Key).First());
            }

            return View(itemStats);
        }

        public IActionResult UserStats(int id)
        {

            _logger.LogInformation($"Showing stats for user with id={id}");

            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserDto, UserViewModel>();
                cfg.CreateMap<ItemDto, ItemViewModel>();
                cfg.CreateMap<CategoryDto, CategoryViewModel>();
                cfg.CreateMap<AddressDto, AddressViewModel>();
                cfg.CreateMap<OperationDto, OperationViewModel>();
            }).CreateMapper();

            var user = _userService.Find(id);

            if (user is null)
                return Redirect(Request.Headers["Referer"].ToString());

            var userStats = new UserStatsViewModel { User = mapper.Map<UserDto, UserViewModel>(user) };

            var oper = _operationService.GetAll();

            userStats.TotalCountForSell = oper.Where(op => op.SellingUser.Id == user.Id).Select(op => op.ItemCount).Sum();
            userStats.TotalValueForSell = oper.Where(op => op.SellingUser.Id == user.Id).Select(op => op.Value).Sum();

            userStats.TotalCountForBuy = oper.Where(op => op.BuyingUser.Id == user.Id).Select(op => op.ItemCount).Sum();
            userStats.TotalValueForBuy = oper.Where(op => op.BuyingUser.Id == user.Id).Select(op => op.Value).Sum();

            userStats.Items = mapper.Map<IEnumerable<ItemDto>, List<ItemViewModel>>(oper.Where(op => op.BuyingUser.Id == user.Id || op.SellingUser.Id == user.Id).Select(op => op.Item).ToList());
            userStats.Operations = mapper.Map<IEnumerable<OperationDto>, List<OperationViewModel>>(oper.Where(op => op.BuyingUser.Id == user.Id || op.SellingUser.Id == user.Id).ToList());

            return View(userStats);
        }

        public IActionResult Privacy()
        {
            _logger.LogInformation("Viewing privacy information");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
