using System;

namespace App.Domain.BLL.DTO
{
    public class OperationDto
    { 
        public long Id { get; set; }
        public UserDto BuyingUser { get; set; }
        public UserDto SellingUser { get; set; }
        public int ItemCount { get; set; }
        public float Value { get; set; }
        public ItemDto Item { get; set; }
        public DateTime SellingDate { get; set; }
    }
}
