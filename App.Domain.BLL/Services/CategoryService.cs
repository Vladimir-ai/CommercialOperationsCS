using App.Domain.BLL.DTO;
using App.Domain.BLL.Infrastructure;
using App.Domain.BLL.Interfaces;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace App.Domain.BLL.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Item> _itemRepository;

        public CategoryService(IRepository<Category> repo, IRepository<Item> itemRepository)
        {
            _categoryRepository = repo;
            _itemRepository = itemRepository;
        }


        public long AddOrUpdate(CategoryDto dto)
        {
            var category = _categoryRepository
                .Find(cat => cat.Name.Equals(dto.Name)).FirstOrDefault();

            if (category is null)
                category = new Category();

            foreach (var itemDto in dto.Items)
            {
                var item = _itemRepository.Find(itemDto.Id);

                if (item is null)
                {
                    item = new Item {Name = itemDto.Name};
                    _itemRepository.CreateOrUpdate(item);
                }
                
                if (category.Items.All(it => it.Id != item.Id))
                    category.Items.Add(item);
            }
            
            _categoryRepository.CreateOrUpdate(category);
            return category.Id;
        }

        public CategoryDto Find(long id)
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Category, CategoryDto>();
                cfg.CreateMap<Item, ItemDto>();
            }).CreateMapper();

            return mapper.Map<Category, CategoryDto>(_categoryRepository.Find(id));
        }

        public IList<CategoryDto> FromStringListIgnoreItems(IEnumerable<string> categoryNames)
        {
            IList<CategoryDto> categories = new List<CategoryDto>();
            
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Category, CategoryDto>()
                .ForMember(cat => cat.Items, dest => dest.Ignore());
            }).CreateMapper();

            foreach(var str in categoryNames)
            {
                if (!_categoryRepository.Exists(item => item.Name.Equals(str)))
                    _categoryRepository.Create(new Category { Name = str });

                var category = _categoryRepository
                    .Find(item => item.Name.Equals(str))
                    .First();
                
                var categoryDto = mapper.Map<Category, CategoryDto>(category);
                categories.Add(categoryDto);
            }

            return categories;
        }

        public List<CategoryDto> GetAll()
        {
            var mapper = new MapperConfiguration(cfg => { 
                cfg.CreateMap<Item, ItemDto>(); 
                cfg.CreateMap<Category, CategoryDto>(); 
            }).CreateMapper();
            
            return mapper.Map<IList<Category>, List<CategoryDto>>(_categoryRepository.GetAll());
        }

        public void Delete(CategoryDto categoryDto)
        {
            Category category = _categoryRepository.Find(dbItem => 
                dbItem.Name.Equals(categoryDto.Name)).FirstOrDefault();
            
            if(category is not null)
                _categoryRepository.Delete(category.Id);
        }
    }
}
