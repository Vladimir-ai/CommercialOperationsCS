using App.Domain.BLL.DTO;
using System.Collections.Generic;

namespace App.Domain.BLL.Interfaces
{
    public interface IItemService : IBaseService<ItemDto>
    {
        int GetTotalAmount(long id);
        
        float GetTotalValue(long id);
        void Delete(long id);
    }
}
