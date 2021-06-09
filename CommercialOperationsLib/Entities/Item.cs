using Core.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public class Item : BaseEntity
    {
        [Required]
        public string Name { get; set; }

        public virtual IList<Operation> Operations { get; set; } = new List<Operation>();
        public virtual IList<Category> Categories { get; set; } = new List<Category>();
        public virtual List<CategoryItem> CategoryItem { get; set; } = new List<CategoryItem>();
    }
}
