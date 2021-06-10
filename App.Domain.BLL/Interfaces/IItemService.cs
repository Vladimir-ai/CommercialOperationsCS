using App.Domain.BLL.DTO;
using System;
using System.Collections.Generic;

namespace App.Domain.BLL.Interfaces
{
    public interface IItemService : IBaseService<ItemDto>
    {
        int GetTotalAmount(long id);
        
        float GetTotalValue(long id);
        int GetTotalAmountWithinDate(long id, DateTime startDate, DateTime endDate);
        float GetTotalValueWithinDate(long id, DateTime startDate, DateTime endDate);
        void Delete(long id);
    }
}
