using Core.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class User : BaseEntity
    {
        public string Name { get; set; }
        
        [ForeignKey("UserTypeId")]
        public virtual UserType UserType { get; set; }
        public long UserTypeId { get; set; }
        
        [ForeignKey("BuildingId")]
        public virtual Building Building { get; set; }

        [InverseProperty("BuyingUser")]
        public virtual IList<Operation> BuyingOperations { get; set; } = new List<Operation>();

        [InverseProperty("SellingUser")]
        public virtual IList<Operation> SellingOperations { get; set; } = new List<Operation>();
        public long BuildingId { get; set; }
    }
}
