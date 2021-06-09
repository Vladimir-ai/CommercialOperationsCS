using App.Domain.BLL.DTO;
using App.Domain.BLL.Interfaces;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Domain.BLL.Services
{
    public class AddressService : IAddressService
    {
        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<City> _cityRepository;
        private readonly IRepository<Street> _streetRepository;
        private readonly IRepository<Building> _buildingRepository;

        public AddressService(IRepository<Country> countryRepo,
            IRepository<City> cityRepo,
            IRepository<Street> streetRepo,
            IRepository<Building> buildingRepo)
        {
            _countryRepository = countryRepo;
            _cityRepository = cityRepo;
            _streetRepository = streetRepo;
            _buildingRepository = buildingRepo;
        }

        public long AddOrUpdate(AddressDto dto)
        {
            Country country = _countryRepository.Find(country => country.Name.Equals(dto.Country)).FirstOrDefault();
            
            if (country is null)
            {
                _countryRepository.Create(new Country() {Name = dto.Country});
            
                country = _countryRepository
                    .Find(country => country.Name.Equals(dto.Country))
                    .FirstOrDefault();
            }

            
            City city = _cityRepository
                .Find(city => city.Name.Equals(dto.City) && city.CountryId == country.Id)
                .FirstOrDefault();
            
            if (city is null)
            {
                _cityRepository
                    .Create(new City() {Name = dto.City, CountryId = country.Id});
                
                city = _cityRepository
                    .Find(city => city.Name.Equals(dto.City) && city.CountryId == country.Id)
                    .FirstOrDefault();
            }

            
            Street street = _streetRepository
                .Find(street => street.Name.Equals(dto.Street) && street.CityId == city.Id)
                .FirstOrDefault();
            
            if (street is null)
            {
                _streetRepository.Create(new Street() {Name = dto.Street, CityId = city.Id});
            
                street = _streetRepository
                    .Find(street => street.Name.Equals(dto.Street) && street.CityId == city.Id)
                    .FirstOrDefault();
            }

            
            Building building = _buildingRepository
                .Find(building => building.Name.Equals(dto.Building) && building.StreetId == street.Id)
                .FirstOrDefault();

            if (building is null)
            {
                building = new Building() {Name = dto.Building, StreetId = street.Id};
                _buildingRepository.Create(building);
            }

            return building.Id;
        }

        public AddressDto Find(long buildingId)
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Building, AddressDto>()
                    .ForMember(dto => dto.Building, src => src.MapFrom(b => b.Name))
                    .ForMember(dto => dto.Street, src => src.MapFrom(b => b.Street.Name))
                    .ForMember(dto => dto.City, src => src.MapFrom(b => b.Street.City.Name))
                    .ForMember(dto => dto.Country, src => src.MapFrom(b => b.Street.City.Country.Name));
            }).CreateMapper();

            Building build = _buildingRepository.Find(buildingId);

            return mapper.Map<Building, AddressDto>(build);
        }

        
        public void AddOrUpdateFromString(string address)
        {
            var addressDelimited =
                address.Split(',', 4, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (addressDelimited.Length != 4)
                throw new ArgumentException("You need 4 comma-separated values");
            
            var addressDto = new AddressDto
            {
                Building = addressDelimited[3],
                Street = addressDelimited[2],
                City = addressDelimited[1],
                Country = addressDelimited[0]
            };
            
            AddOrUpdate(addressDto);
        }
        
        private Building GetBuildingFromDB(AddressDto address)
        {
            Country country = _countryRepository
                .Find(country => country.Name.Equals(address.Country))
                .FirstOrDefault();
            
            if (country is null)
                return null;

            
            City city = _cityRepository
                .Find(city => city.Name.Equals(address.City) && city.CountryId == country.Id)
                .FirstOrDefault();
            
            if (city is null)
                return null;

            
            Street street = _streetRepository
                .Find(street => street.Name.Equals(address.Street) && street.CityId == city.Id)
                .FirstOrDefault();
            if (street is null)
                return null;

            
            return _buildingRepository
                .Find(building => building.Name.Equals(address.Building) && building.StreetId == street.Id)
                .FirstOrDefault();
        }

        public List<AddressDto> GetAll()
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Building, AddressDto>()
                    .ForMember(dto => dto.Building, src => src.MapFrom(b => b.Name))
                    .ForMember(dto => dto.Street, src => src.MapFrom(b => b.Street.Name))
                    .ForMember(dto => dto.City, src => src.MapFrom(b => b.Street.City.Name))
                    .ForMember(dto => dto.Country, src => src.MapFrom(b => b.Street.City.Country.Name));
            }).CreateMapper();

            var res = _buildingRepository.GetAllInclude(b => b.Street,
                b => b.Street.City, b => b.Street.City.Country);

            return mapper.Map<IEnumerable<Building>, List<AddressDto>>(res);
        }

        public void Delete(AddressDto address)
        {
            var building = GetBuildingFromDB(address);
            
            if (building is not null)
                _buildingRepository.Delete(building.Id);
            
        }
    }
}