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
                Random random = new Random();
                var itemRepo = new Repository<Item>(db);
                var userRepo = new Repository<User>(db);
                var opRepo = new Repository<Operation>(db);

                var allItems = itemRepo.GetAll();
                var allUsers = userRepo.GetAll();
                
                for (int i = 0; i < 50; i++)
                {
                    var sellingUser = allUsers[random.Next(allUsers.Count)];
                    var buyingUser = allUsers[random.Next(allUsers.Count)];
                    while (buyingUser.Id == sellingUser.Id)
                    {
                        buyingUser = allUsers[random.Next(allUsers.Count)];
                    }

                    var item = allItems[random.Next(allItems.Count)];

                    var price = 10000 - random.NextDouble() * 10000;
                    var amount = random.Next(100);
                    
                    DateTime start = new DateTime(2010, 1, 1);
                    int range = (DateTime.Today - start).Days;           
                    var date = start.AddDays(random.Next(range));

                    var operation = new Operation { BuyingUser = buyingUser, 
                        SellingUser = sellingUser, Item = item, Value = (float) price,
                        ItemCount = amount, SellingDate = date};
                    
                    opRepo.Create(operation);
                }
                
            }

            Console.WriteLine("ok");
        }
    }
}
