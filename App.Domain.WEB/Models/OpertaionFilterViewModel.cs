using System;
using System.Collections.Generic;
using System.Linq;
using App.Domain.WEB.Utils;

namespace App.Domain.WEB.Models
{
    public class OperationFilterViewModel
    {
        [StringInterceptor] public string SortOrder { get; set; } = "";
        [StringArrayInterceptor] public string[] ItemNameFilter { get; set; } = Array.Empty<string>();

        [StringArrayInterceptor] public string[] CatFilter { get; set; } = Array.Empty<string>();

        [StringInterceptor] public string SellUsrNameFilter { get; set; } = "";
        [StringInterceptor] public string BuyUsrNameFilter { get; set; } = "";

        public int MinAmount { get; set; } = 0;
        public int MaxAmount { get; set; } = int.MaxValue;

        public float MinValue { get; set; } = 0;
        public float MaxValue { get; set; } = float.MaxValue;

        public DateTime StartDate { get; set; } = DateTime.MinValue;
        public DateTime EndDate = DateTime.MaxValue;
        
        // public string Group { get; set; } = "";

        public void SortUsingOrder(ref List<OperationViewModel> operationList)
        {
            operationList = SortOrder switch
            {
                "id_desc" => operationList.AsQueryable().Reverse().ToList(),
                "ItemName" => operationList.OrderBy(r => r.Item.Name).ToList(),
                "name_desc" => operationList.OrderByDescending(r => r.Item.Name).ToList(),
                "ItemCat" => operationList.OrderBy(r => r.Item.Categories[0].Name).ToList(),
                "cat_name_desc" => operationList.OrderByDescending(r => r.Item.Categories[0].Name).ToList(),
                "ItemCount" => operationList.OrderBy(r => r.ItemCount).ToList(),
                "count_desc" => operationList.OrderByDescending(r => r.ItemCount).ToList(),
                "ItemValue" => operationList.OrderBy(r => r.Value).ToList(),
                "value_desc" => operationList.OrderByDescending(r => r.Value).ToList(),
                "BuyUser" => operationList.OrderBy(r => r.SellingUser.Name).ToList(),
                "buy_usr_desc" => operationList.OrderByDescending(r => r.SellingUser.Name).ToList(),
                "SellUser" => operationList.OrderBy(r => r.BuyingUser.Name).ToList(),
                "sell_usr_desc" => operationList.OrderBy(r => r.BuyingUser.Name).ToList(),
                "Date" => operationList.OrderBy(r => r.SellingDate).ToList(),
                "date_desc" => operationList.OrderByDescending(r => r.SellingDate).ToList(),
                _ => operationList
            };
        }

        // public void ApplySort(ref List<OperationFilterViewModel> operationList)
        // {
        //     operationList = Group switch
        //     {
        //         "Buying User" => operationList.
        //         _ => operationList
        //     };
        //
        // }
        
        public void SortByAmount(ref List<OperationViewModel> operationList)
        {
            operationList = operationList
                .Where(it => it.ItemCount <= MaxAmount)
                .Where(it => it.ItemCount >= MinAmount)
                .ToList();

            operationList = operationList
                .Where(it => it.Value <= MaxValue)
                .Where(it => it.Value >= MinValue)
                .ToList();

            operationList = operationList
                .Where(it => it.SellingDate <= EndDate)
                .Where(it => it.SellingDate >= StartDate)
                .ToList();
        }
        public void SortByCatUsr(ref List<OperationViewModel> operationList)
        {
            if (ItemNameFilter.Length > 0)
                operationList = operationList.Where(oper =>
                    ItemNameFilter.Contains(oper.Item.Name)).ToList();

            if (BuyUsrNameFilter.Length > 0)
                operationList = operationList.Where(oper =>
                    oper.BuyingUser.Name.Contains(BuyUsrNameFilter)).ToList();

            if (SellUsrNameFilter.Length != 0)
                operationList = operationList.Where(oper =>
                    oper.SellingUser.Name.Contains(SellUsrNameFilter)).ToList();

            if (CatFilter.Length > 0)
                operationList = operationList.Where(oper =>
                    oper.Item.Categories.Select(cat => cat.Name)
                        .Intersect(CatFilter).Any()).ToList();
        }
    }
}