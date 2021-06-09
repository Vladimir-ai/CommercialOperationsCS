using System.Collections.Generic;

namespace App.Domain.BLL.DTO
{
    public class ItemDto
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public IList<CategoryDto> Categories { get; set; }
    }
}
