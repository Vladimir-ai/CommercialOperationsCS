using App.Domain.BLL.DTO;
using App.Domain.BLL.Infrastructure;
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
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserType> _usrTypeRepository;
        private readonly IRepository<Operation> _operationRepository;

        private readonly IAddressService _addressService;

        public UserService(IRepository<User> userRepo, 
            IRepository<UserType> userTypeRepo,
            IRepository<Operation> operationRepository, IAddressService addressService)
        {
            _userRepository = userRepo;
            _usrTypeRepository = userTypeRepo;
            _operationRepository = operationRepository;
            _addressService = addressService;
        }

        public long AddOrUpdate(UserDto dto)
        {
            var user = _userRepository.Find(dto.Id);

            if (user is null)
            {
                user = new User {Name = dto.Name};
            }


            user.Name = dto.Name;
            user.UserType = _usrTypeRepository
                .Find(type => type.Type.Equals(dto.UserType)).First();


            user.BuildingId = _addressService.AddOrUpdate(dto.Address);
            
            _userRepository.CreateOrUpdate(user);
            return user.Id;
        }


        public UserDto Find(long id)
        {
            var mapper = new MapperConfiguration(cfg => { 
                cfg.CreateMap<User, UserDto>()
                    .ForPath(user => user.Address.Building, src => src.MapFrom(usr => usr.Building.Name))
                    .ForPath(user => user.Address.Street, src => src.MapFrom(usr => usr.Building.Street.Name))
                    .ForPath(user => user.Address.City, src => src.MapFrom(usr => usr.Building.Street.City.Name))
                    .ForPath(user => user.Address.Country, src => src.MapFrom(usr => usr.Building.Street.City.Country.Name))
                    .ForPath(user => user.UserType, src => src.MapFrom(usr => usr.UserType.Type));
            }).CreateMapper();

            User usr = _userRepository.Find(id);
            return mapper.Map<User, UserDto>(usr);
        }

        public List<UserDto> GetAll()
        {
            var mapper = new MapperConfiguration(cfg => {
                cfg.CreateMap<User, UserDto>()
                    .ForPath(user => user.Address.Building, src => src.MapFrom(usr => usr.Building.Name))
                    .ForPath(user => user.Address.Street, src => src.MapFrom(usr => usr.Building.Street.Name))
                    .ForPath(user => user.Address.City, src => src.MapFrom(usr => usr.Building.Street.City.Name))
                    .ForPath(user => user.Address.Country, src => src.MapFrom(usr => usr.Building.Street.City.Country.Name))
                    .ForPath(user => user.UserType, src => src.MapFrom(usr => usr.UserType.Type));
            }).CreateMapper();

            return mapper.Map<IEnumerable<User>, List<UserDto>>(_userRepository.GetAll());
        }

        public IEnumerable<string> GetAvailableUserTypes()
        {
            return _usrTypeRepository.GetAll().Select(type => type.Type).ToList();
        }

        public void Delete(long id)
        {
            var usr = _userRepository.Find(id);
            if (usr is not null)
            {
                usr.BuyingOperations.ToList()
                    .ForEach(op => _operationRepository.Delete(op.Id));
                
                usr.SellingOperations.ToList()
                    .ForEach(op => _operationRepository.Delete(op.Id));
                
                _userRepository.Delete(id);
            }
        }

        public float? GetTotalBoughtValueById(long id)
        {
            return _userRepository.Find(id)?.BuyingOperations.Select(op => op.Value).Sum();
        }

        public float? GetTotalSoldValueById(long id)
        {
            return _userRepository.Find(id)?.SellingOperations.Select(op => op.Value).Sum();
        }

        public int? GetTotalBoughtAmountById(long id)
        {
            return _userRepository.Find(id)?.BuyingOperations.Select(op => op.ItemCount).Sum();
        }

        public int? GetTotalSoldAmountById(long id)
        {
            return _userRepository.Find(id)?.SellingOperations.Select(op => op.ItemCount).Sum();
        }
    }
}
