using System;
using System.Collections.Generic;
using System.Linq;
using App.Domain.WEB.Utils;

namespace App.Domain.WEB.Models
{
    public class UsersFilterViewModel
    {
        public string SortOrder { get; set; } = "";
        public string NameFilter { get; set; } = "";
        public string TypeFilter { get; set; } = "";
        public string AddressFilter { get; set; } = "";

        public int MinBoughtItemAmount { get; set; } = 0;
        public int MaxBoughtItemAmount { get; set; } = int.MaxValue;

        public int MinSoldItemAmount { get; set; } = 0;
        public int MaxSoldItemAmount { get; set; } = int.MaxValue;

        public float MinBoughtItemValue { get; set; } = 0;
        public float MaxBoughtItemValue { get; set; } = float.MaxValue;

        public float MinSoldItemValue { get; set; } = 0;
        public float MaxSoldItemValue { get; set; } = float.MaxValue;

        public DateTime StartDate { get; set; } = DateTime.MinValue;
        public DateTime EndDate { get; set; } = DateTime.MaxValue;


        public void SortUsingOrder(ref List<UserViewModel> usersList)
        {
            usersList = SortOrder switch
            {
                "id_desc" => usersList.AsQueryable().Reverse().ToList(),
                "Name" => usersList.OrderBy(r => r.Name).ToList(),
                "name_desc" => usersList.OrderByDescending(r => r.Name).ToList(),
                "UserType" => usersList.OrderBy(r => r.UserType).ToList(),
                "usertype_desc" => usersList.OrderByDescending(r => r.UserType).ToList(),
                "Address" => usersList.OrderBy(r => r.Address.ToString()).ToList(),
                "address_desc" => usersList.OrderByDescending(r => r.Address.ToString()).ToList(),
                "TotalBoughtAmount" => usersList.OrderBy(r => r.TotalBoughtAmount).ToList(),
                "total_bought_am_desc" => usersList.OrderByDescending(r => r.TotalBoughtAmount).ToList(),
                "TotalSoldAmount" => usersList.OrderBy(r => r.TotalSoldAmount).ToList(),
                "total_sold_am_desc" => usersList.OrderByDescending(r => r.TotalSoldAmount).ToList(),
                "TotalBoughtValue" => usersList.OrderBy(r => r.TotalBoughtValue).ToList(),
                "total_bought_val_desc" => usersList.OrderByDescending(r => r.TotalBoughtValue).ToList(),
                "TotalSoldValue" => usersList.OrderBy(r => r.TotalSoldValue).ToList(),
                "total_sold_val_desc" => usersList.OrderByDescending(r => r.TotalSoldValue).ToList(),
                _ => usersList
            };
        }

        public void ApplyFilter(ref List<UserViewModel> usersList)
        {
            if (!string.IsNullOrEmpty(NameFilter))
                usersList = usersList
                    .FindAll(user => user
                        .Name.Contains(NameFilter, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(TypeFilter))
                usersList = usersList.FindAll(user => user.UserType.Contains(TypeFilter));

            if (!string.IsNullOrEmpty(AddressFilter))
                usersList = usersList
                    .FindAll(user => user.Address.ToString()
                        .Contains(AddressFilter, StringComparison.OrdinalIgnoreCase));
        }

        public void FilterByAmounts(ref List<UserViewModel> usersList)
        {
            usersList = usersList
                .Where(user => user.TotalBoughtAmount >= MinBoughtItemAmount)
                .Where(user => user.TotalBoughtAmount <= MaxBoughtItemAmount)
                .ToList();

            usersList = usersList
                .Where(user => user.TotalSoldAmount >= MinSoldItemAmount)
                .Where(user => user.TotalSoldAmount <= MaxSoldItemAmount)
                .ToList();

            usersList = usersList
                .Where(user => user.TotalBoughtValue >= MinBoughtItemValue)
                .Where(user => user.TotalBoughtValue <= MaxBoughtItemValue)
                .ToList();

            usersList = usersList
                .Where(user => user.TotalSoldValue >= MinSoldItemValue)
                .Where(user => user.TotalSoldValue <= MaxSoldItemValue)
                .ToList();

        }
    }
}