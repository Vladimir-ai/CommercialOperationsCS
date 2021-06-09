using Core.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public abstract class BaseEntity : IBaseEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
    }
}
