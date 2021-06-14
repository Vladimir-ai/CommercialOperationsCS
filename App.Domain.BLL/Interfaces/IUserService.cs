using App.Domain.BLL.DTO;
using System;
using System.Collections.Generic;

namespace App.Domain.BLL.Interfaces
{
    public interface IUserService : IBaseService<UserDto>
    {
        IEnumerable<string> GetAvailableUserTypes();

        void Delete(long id);

        float? GetTotalBoughtValueByIdAndDate(long id, DateTime startDate, DateTime endDate);
        float? GetTotalSoldValueByIdAndDate(long id, DateTime startDate, DateTime endDate);
        int? GetTotalBoughtAmountByIdAndDate(long id, DateTime startDate, DateTime endDate);
        int? GetTotalSoldAmountByIdAndDate(long id, DateTime startDate, DateTime endDate);
    }
}
