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

namespace App.Domain.BLL.Services
{
    public class ItemService : IItemService
    {
        private readonly IRepository<Item> _itemRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Operation> _opertaionRepository;

        public ItemService(IRepository<Item> itRepo, IRepository<Category> catRepo,
            IRepository<Operation> operationRepo)
        {
            _itemRepository = itRepo;
            _categoryRepository = catRepo;
            _opertaionRepository = operationRepo;
        }

        public long AddOrUpdate(ItemDto dto)
        {
            var item = _itemRepository.Find(dto.Id);

            if (item is null)
                item = new Item();

            item.Name = dto.Name;

            foreach (var deleted in item.Categories.Where(cat => dto.Categories.All(catDt => !catDt.Name.Equals(cat.Name))).ToList())
            {
                item.Categories.Remove(deleted);
            }

            foreach (var catDto in dto.Categories)
            {
                var category = _categoryRepository.Find(cat => cat.Name.Equals(catDto.Name)).FirstOrDefault();

                if (category is null)
                {
                    category = new Category {Name = catDto.Name};
                    _categoryRepository.Create(category);
                }

                if (item.Categories.All(cat => !cat.Name.Equals(catDto.Name)))
                    item.Categories.Add(category);

                
            }

            _itemRepository.CreateOrUpdate(item);
            return item.Id;
        }

        public ItemDto Find(long id)
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Item, ItemDto>();
                cfg.CreateMap<Category, CategoryDto>();
            }).CreateMapper();

            Item itm = _itemRepository.Find(id);
            var item = mapper.Map<Item, ItemDto>(itm);

            return item;
        }

        public List<ItemDto> GetAll()
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Item, ItemDto>();
                cfg.CreateMap<Category, CategoryDto>();
            }).CreateMapper();

            return mapper.Map<IEnumerable<Item>, List<ItemDto>>(_itemRepository.GetAll());
        }

        public int GetTotalAmount(long id)
        {
            var item = _itemRepository.Find(id);
           
            return item?.Operations
                .Select(it => it.ItemCount).Sum() ?? 0;
        }

        public float GetTotalValue(long id)
        {
            var item = _itemRepository.Find(id);

            return item?.Operations
                .Select(it => it.Value).Sum() ?? 0;
        }


        public int GetTotalAmountWithinDate(long id, DateTime startDate, DateTime endDate)
        {
            var item = _itemRepository.Find(id);

            return item?.Operations
                .Where(op => op.SellingDate <= endDate)
                .Where(op => op.SellingDate >= startDate)
                .Select(op => op.ItemCount).Sum() ?? 0;
        }

        public float GetTotalValueWithinDate(long id, DateTime startDate, DateTime endDate)
        {
            var item = _itemRepository.Find(id);

            return item?.Operations
                .Where(op => op.SellingDate <= endDate)
                .Where(op => op.SellingDate >= startDate)
                .Select(op => op.Value).Sum() ?? 0;
        }

        public void Delete(long id)
        {
            var toDelete = _itemRepository.Find(id);

            if (toDelete is not null)
                _itemRepository.Delete(id);
        }

        
    }
}