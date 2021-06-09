using App.Domain.BLL.DTO;
using App.Domain.BLL.Interfaces;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Domain.BLL.Services
{
    public class OperationService : IOperationService
    {
        private readonly IRepository<Operation> _operationRepository;
        private readonly IRepository<Item> _itemRepository;
        private readonly IRepository<User> _userRepository;
        
        private readonly IItemService _itemService;
        private readonly IUserService _userService;


        public OperationService(IRepository<Operation> opRepo, 
            IRepository<Item> itemRepository, IItemService itemService, 
            IRepository<User> userRepository, IUserService userService)
        {
            _operationRepository = opRepo;
            _itemRepository = itemRepository;
            _itemService = itemService;
            _userRepository = userRepository;
            _userService = userService;
        }

        public IList<OperationDto> BuyOperations(UserDto user)
        {
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<OperationDto, Operation>()).CreateMapper();
            IList<Operation> operations = _operationRepository.FindInclude(usr => usr.SellingUser.Id == user.Id,  oper => oper.SellingUser).ToList();
            return mapper.Map<IList<Operation>, List<OperationDto>>(operations); 
        }

        public long AddOrUpdate(OperationDto dto)
        {
            var operation = _operationRepository.Find(dto.Id);

            if (operation is null)
            {
                operation = new Operation {Value = dto.Value, 
                    ItemCount = dto.ItemCount, 
                    SellingDate = dto.SellingDate};
            }

            operation.Value = dto.Value;
            operation.ItemCount = dto.ItemCount;
            operation.SellingDate = dto.SellingDate;

            var item = _itemRepository.Find(dto.Item.Id);
            var buyingUser = _userRepository.Find(dto.BuyingUser.Id);
            var sellingUser = _userRepository.Find(dto.SellingUser.Id);
            
            if (item is null)
                item = _itemRepository.Find(_itemService.AddOrUpdate(dto.Item));
            
            if (buyingUser is null)
                buyingUser = _userRepository.Find(_userService.AddOrUpdate(dto.BuyingUser));

            if (sellingUser is null)
                sellingUser = _userRepository.Find(_userService.AddOrUpdate(dto.SellingUser));
            
            operation.Item = item;
            operation.BuyingUser = buyingUser;
            operation.SellingUser = sellingUser;
            
            _operationRepository.CreateOrUpdate(operation);
            return operation.Id;
        }

        public OperationDto Find(long id)
        {
            var mapper = new MapperConfiguration(cfg => {
                cfg.CreateMap<Operation, OperationDto>();
                cfg.CreateMap<User, UserDto>().ForMember(user => user.UserType, dst => dst.MapFrom(userDTO => userDTO.UserType.Type));
                cfg.CreateMap<Category, CategoryDto>();
                cfg.CreateMap<Item, ItemDto>();
            }).CreateMapper();

            return mapper.Map<Operation, OperationDto>(_operationRepository.Find(id));
        }

        public List<OperationDto> GetAll()
        {
            var mapper = new MapperConfiguration(cfg => {
                cfg.CreateMap<Operation, OperationDto>();
                cfg.CreateMap<User, UserDto>().ForMember(user => user.UserType, dst => dst.MapFrom(userDTO => userDTO.UserType.Type));
                cfg.CreateMap<Category, CategoryDto>();
                cfg.CreateMap<Item, ItemDto>();
            }).CreateMapper();

            var res = _operationRepository.GetAll();

            return mapper.Map<IList<Operation>, List<OperationDto>>(res);
        }

        public IList<OperationDto> FindAllOperationsByCategory(CategoryDto category)
        {
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<CategoryDto, Category>()).CreateMapper();
            var categoryEntity = mapper.Map<CategoryDto, Category>(category);

            IList<Operation> operations = _operationRepository.FindInclude(oper => oper.Item.Categories.Contains(categoryEntity),
                oper => oper.Item.Categories);

            mapper = new MapperConfiguration(cfg => { 
                cfg.CreateMap<Operation, OperationDto>();
                cfg.CreateMap<User, UserDto>();
                cfg.CreateMap<Category, CategoryDto>();
                cfg.CreateMap<Item, ItemDto>();
            }).CreateMapper();

            return mapper.Map<IList<Operation>, List<OperationDto>>(operations);
        }

        public IList<OperationDto> SellOperations(UserDto user)
        {
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<OperationDto, Operation>()).CreateMapper();
            IList<Operation> operations = _operationRepository.FindInclude(usr => usr.BuyingUser.Id == user.Id, oper => oper.BuyingUser).ToList();
            return mapper.Map<IList<Operation>, List<OperationDto>>(operations);
        }

        public List<OperationDto> GetUserSellingOperations(UserDto user)
        {
            return new List<OperationDto>();//TODO
        }

        public List<OperationDto> GetUserBuyingOperations(UserDto user)
        {
            return new List<OperationDto>();//TODO
        }

        public void Delete(long id)
        {
            _operationRepository.Delete(id);
        }
    }
}
