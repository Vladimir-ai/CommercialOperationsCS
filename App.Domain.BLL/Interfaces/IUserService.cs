using App.Domain.BLL.DTO;
using System.Collections.Generic;

namespace App.Domain.BLL.Interfaces
{
    public interface IUserService : IBaseService<UserDto>
    {
        IEnumerable<string> GetAvailableUserTypes();

        void Delete(long id);
    }
}
