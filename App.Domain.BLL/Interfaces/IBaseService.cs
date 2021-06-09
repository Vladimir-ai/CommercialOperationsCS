using System.Collections.Generic;

namespace App.Domain.BLL.Interfaces
{
    public interface IBaseService<T> where T: class
    {
        long AddOrUpdate(T dto);

        T Find(long id);
        
        List<T> GetAll();

    }
}