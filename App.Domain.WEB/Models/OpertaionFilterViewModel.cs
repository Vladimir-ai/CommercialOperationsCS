using System;
using System.Collections.Generic;
using App.Domain.WEB.Utils;

namespace App.Domain.WEB.Models
{
    public class OperationFilterViewModel
    {
        public string SortOrder { get; set; }

        [StringArrayInterceptor]
        public string[] ItemNameFilter { get; set; }

        [StringArrayInterceptor]
        public string[] CatFilter { get; set; } 
        
        public string SellUsrNameFilter { get; set; }
        public string BuyUsrNameFilter { get; set; }

        public int MinAmount { get; set; } = 0;
        public int MaxAmount { get; set; } = int.MaxValue;

        public float MinValue { get; set; } = 0;
        public float MaxValue { get; set; } = float.MaxValue;
        
        public DateTime StartDate { get; set; } = DateTime.MinValue;
        public DateTime EndDate = DateTime.MaxValue;
        
        public bool Group { get; set; }

        public List<OperationViewModel> MakeOrder(List<OperationViewModel> operationList)
        {
            
        }
    }
}