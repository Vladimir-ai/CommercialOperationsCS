using Core.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class Operation : BaseEntity
    {
        [ForeignKey("BuyingUser")]
        public long BuyingUserId { get; set; }
        
        [ForeignKey("SellingUser")]
        public long SellingUserId { get; set; }
        public virtual User BuyingUser { get; set; }
        public virtual User SellingUser { get; set; }

        public int ItemCount { get; set; }
        public float Value { get; set; }
        
        [Required]
        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; }
        public long ItemId { get; set; }
        
        [Required]
        public DateTime SellingDate { get; set; }
    }
}
