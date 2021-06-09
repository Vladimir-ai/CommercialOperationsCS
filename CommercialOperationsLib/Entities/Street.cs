using Core.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class Street : BaseEntity
    {
        public string Name { get; set; }
        public virtual IList<Building> Buildings { get; set; } = new List<Building>();
        [ForeignKey("CityId")]
        public virtual City City { get; set; }
        [Required]
        public long CityId { get; set; }
    }
}
