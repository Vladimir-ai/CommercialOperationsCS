using Core.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public class Category : BaseEntity
    {
        [Required]
        public string Name { get; set; } //unique
        public virtual IList<Item> Items { get; set; } = new List<Item>();
        public virtual List<CategoryItem> CategoryItem { get; set; } = new List<CategoryItem>();
    }
}
