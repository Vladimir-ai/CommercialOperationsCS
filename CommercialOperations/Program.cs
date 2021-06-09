using App.Domain.BLL;
using App.Domain.BLL.DTO;
using App.Domain.BLL.Infrastructure;
using App.Domain.BLL.Interfaces;
using App.Domain.BLL.Services;
using Core.Entities;
using Core.Interfaces;
using Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.EF;

namespace CommercialOperations
{
    class Program
    {
        static void Main(string[] args)
        {
            using (MyContext db = new MyContext(new DbContextOptions<MyContext>()))
            {
                var userRepo = new Repository<User>(db);
                var userTypeRepo = new Repository<UserType>(db);
                var buildingRepo = new Repository<Building>(db);
                var countyRepo = new Repository<Country>(db);
                var operRepo = new Repository<Operation>(db);
                var itemRepo = new Repository<Item>(db);


                var cat = new Category {Name = "aaa"};
                var item = new Item {Name = "aaaItem"};
                item.Categories.Add(cat);

                var count = new Country {Name = "AAAA"};
                var city = new City {Name = "BBBB", Country = count};
                var street = new Street {Name = "CCCC", City = city};
                var building = new Building {Name = "DDDD", Street = street};
                var anotherBuilding = new Building {Name = "SSSS", Street = street};

                var userType = new UserType {Type = "Homeless"};

                buildingRepo.CreateOrUpdate(building);
                buildingRepo.CreateOrUpdate(anotherBuilding);
                userTypeRepo.CreateOrUpdate(userType);

                var usr1 = new User {Building = building, Name = "AAAA", UserType = userType};
                var usr2 = new User {Building = anotherBuilding, Name = "BBBB", UserType = userType};

                var oper = new Operation
                {
                    BuyingUser = usr1, SellingUser = usr2, Item = item, ItemCount = 1, Value = 0.02f,
                    SellingDate = DateTime.Now
                };

                operRepo.CreateOrUpdate(oper);
                //     userRepo.Delete(1);
//                itemRepo.Delete(1);

            }

            Console.WriteLine("ok");
        }
    }
}
