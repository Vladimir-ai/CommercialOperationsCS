using App.Domain.BLL.DTO;
using System.Collections.Generic;

namespace App.Domain.BLL.Interfaces
{
    public interface IUserService : IBaseService<UserDto>
    {
        IEnumerable<string> GetAvailableUserTypes();

        void Delete(long id);

        float? GetTotalBoughtValueById(long id);
        float? GetTotalSoldValueById(long id);
        int? GetTotalBoughtAmountById(long id);
        int? GetTotalSoldAmountById(long id);
    }
}
