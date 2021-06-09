using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class CategoryItem : BaseEntity
    {
        public virtual Item Item { get; set; }
        
        [ForeignKey("ItemId")]
        public long ItemId { get; set; }
        
        public virtual Category Category { get; set; }
        [ForeignKey("CategoryId")]
        public long CategoryId { get; set; }
    }
}
