using System;

namespace App.Domain.WEB.Models
{
    public class OperationViewModel
    {
        public long Id { get; set; }
        public UserViewModel BuyingUser { get; set; }
        public UserViewModel SellingUser { get; set; }
        public int ItemCount { get; set; }
        public float Value { get; set; }
        public ItemViewModel Item { get; set; }
        public DateTime SellingDate { get; set; }
    }
}