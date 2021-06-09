using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        IList<T> GetAll();
        T Find(long id);
        IList<T> Find(Func<T, bool> predicate);
        bool Exists(Func<T, bool> predicate);
        int Count();
        int Count(Func<T, bool> predicate);
        void Update(T item); //deprecated
        void Delete(long id);
        void Create(T item);
        void CreateOrUpdate(T item);
        IList<T> FindInclude(Func<T, bool> predicate, params Expression<Func<T, object>>[] includes);
        IList<T> GetAllInclude(params Expression<Func<T, object>>[] includes);
        void UpdateManyToMany(Expression<Func<T, bool>> filter,
            IEnumerable<object> updatedSet, // Updated many-to-many relationships
            IEnumerable<object> availableSet, // Lookup collection
            string propertyName); // The name of the navigation property
    }
}
