using App.Domain.BLL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Domain.BLL.Interfaces
{
    public interface IOperationService : IBaseService<OperationDto>
    {
        List<OperationDto> GetUserSellingOperations(UserDto user);
        List<OperationDto> GetUserBuyingOperations(UserDto user);
        void Delete(long id);
    }
}
