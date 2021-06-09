using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class Country : BaseEntity
    {
        public virtual IList<City> Cities { get; set; } = new List<City>();
        public String Name { get; set; }
    }
}
