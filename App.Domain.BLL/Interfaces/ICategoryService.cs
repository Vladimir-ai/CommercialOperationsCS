using App.Domain.BLL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Domain.BLL.Interfaces
{
    public interface ICategoryService : IBaseService<CategoryDto>
    {
        IList<CategoryDto> FromStringListIgnoreItems(IEnumerable<string> categoryNames);

    }
}
