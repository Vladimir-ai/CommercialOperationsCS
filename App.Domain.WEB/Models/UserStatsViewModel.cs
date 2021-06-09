using System.Collections.Generic;

namespace App.Domain.WEB.Models
{
    public class UserStatsViewModel
    {
        public UserViewModel User { get; set; }
        public List<OperationViewModel> Operations { get; set; }

        public float TotalValueForSell { get; set; }
        public int TotalCountForSell { get; set; }
        public float TotalValueForBuy { get; set; }
        public int TotalCountForBuy { get; set; }
        public List<ItemViewModel> Items {get;set;}
    }
}