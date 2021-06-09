using Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class Building : BaseEntity
    {
        [Required]
        public string Name { get; set; }
        [ForeignKey("StreetId")]
        public virtual Street Street { get; set; }
        [Required]
        public long StreetId { get; set; }
    }
}
