using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class City : BaseEntity
    {
        [Required]
        public String Name { get; set; }

        public virtual IList<Street> Streets { get; set; } = new List<Street>();
        [ForeignKey("CountryId")]
        public virtual Country Country { get; set; }
        [Required]
        public long CountryId { get; set; }
    }
}
