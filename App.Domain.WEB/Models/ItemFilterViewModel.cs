using System;
using System.Collections.Generic;
using System.Linq;
using App.Domain.WEB.Utils;

namespace App.Domain.WEB.Models
{
    public class ItemFilterViewModel
    {
        public string SortOrder { get; set; } = "";

        public string NameFilter { get; set; } = "";

        public string[] CatFilter { get; set; } = Array.Empty<string>();

        public float MinVal { get; set; } = 0;
        public float MaxVal { get; set; } = float.MaxValue;

        public int MinAmount { get; set; } = 0;
        public int MaxAmount { get; set; } = int.MaxValue;

        public DateTime StartDate { get; set; } = DateTime.MinValue;
        public DateTime EndDate { get; set; } = DateTime.MaxValue;

        public void SortUsingOrder(ref List<ItemViewModel> itemList)
        {
            itemList = SortOrder switch
            {
                "id_desc" => itemList.AsQueryable().Reverse().ToList(),
                "Name" => itemList.OrderBy(r => r.Name).ToList(),
                "name_desc" => itemList.OrderByDescending(r => r.Name).ToList(),
                "Category" => itemList.OrderBy(r => r.Categories[0].Name).ToList(),
                "cat_desc" => itemList.OrderByDescending(r => r.Categories[0].Name).ToList(),
                "Amount" => itemList.OrderBy(r => r.TotalAmount).ToList(),
                "amount_desc" => itemList.OrderByDescending(r => r.TotalAmount).ToList(),
                "Value" => itemList.OrderBy(r => r.TotalValue).ToList(),
                "val_desc" => itemList.OrderByDescending(r => r.TotalValue).ToList(),
                _ => itemList
            };
        }

        public void FilterByValue(ref List<ItemViewModel> itemList)
        {

            if (NameFilter != null && !NameFilter.Equals(""))
                itemList = itemList.FindAll(item =>
                    item.Name
                        .Contains(NameFilter,
                            StringComparison.OrdinalIgnoreCase));

            if (CatFilter.Length != 0)
            {
                itemList = itemList.Where(item => item.Categories
                        .Select(cat => cat.Name)
                        .Intersect(CatFilter).Any())
                        .ToList();
            }
        }

        public void FilterByAmount(ref List<ItemViewModel> itemList)
        {
            itemList = itemList
                .Where(it => it.TotalAmount <= MaxAmount && it.TotalAmount >= MinAmount)
                .ToList();

            itemList = itemList
                .Where(it => it.TotalValue <= MaxVal && it.TotalValue >= MinVal)
                .ToList();
        }
    }
}