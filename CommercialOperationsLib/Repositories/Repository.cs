using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Core.EF;

namespace Core.Repositories
{
    public class Repository<T> : IRepository<T> where T : class, IBaseEntity
    {
        private DbContext db;
        private readonly DbSet<T> _entities;

        public Repository(MyContext context)
        {
            db = context;
            _entities = context.Set<T>();
        }
        public void Create(T item)
        {
            _entities.Add(item);
            db.SaveChanges();
        }

        public void CreateOrUpdate(T item)
        {
            if (item.Id == 0 || !_entities.Any(ent => ent.Id == item.Id))
                _entities.Add(item);
            db.SaveChanges();
        }

        public void Delete(long id)
        {
            T temp = Find(id);
            if (temp != null)
                _entities.Remove(temp);
            db.SaveChanges();
        }

        public IList<T> Find(Func<T, bool> predicate)
        {   
            return _entities.Where(predicate).ToList();
        }

        public T Find(long id)
        {
            return _entities.Where(ent => ent.Id == id).FirstOrDefault();
        }

        public IList<T> GetAll()
        {
            return _entities.ToList();
        }

        public void Update(T item)
        {
            
            db.Entry(item).State = EntityState.Modified;
            db.SaveChanges();
            db.ChangeTracker.Clear();
        }

        public void UpdateManyToMany(Expression<Func<T, bool>> filter,
            IEnumerable<object> updatedSet, // Updated many-to-many relationships
            IEnumerable<object> availableSet, // Lookup collection
            string propertyName) // The name of the navigation property
        {

            db.ChangeTracker.Clear();
            // Get the generic type of the set
            var type = updatedSet.GetType().GetGenericArguments()[0];

            // Get the previous entity from the database based on repository type
            var previous = db
                .Set<T>()
                .Include(propertyName)
                .FirstOrDefault(filter);

            /* Create a container that will hold the values of
                * the generic many-to-many relationships we are updating.
                */
            var values = CreateList(type);

            /* For each object in the updated set find the existing
                 * entity in the database. This is to avoid Entity Framework
                 * from creating new objects or throwing an
                 * error because the object is already attached.
                 */

            foreach (var entry in updatedSet)
            {
                values.Add(entry);
            }

            /* Get the collection where the previous many to many relationships
                * are stored and assign the new ones.
                */
            db.Entry(previous).Collection(propertyName).CurrentValue = values;
            db.SaveChanges();
        }


        private IList CreateList(Type type)
        {
            var genericList = typeof(List<>).MakeGenericType(type);
            return (IList)Activator.CreateInstance(genericList);
        }
        
        public IList<T> FindInclude(Func<T, bool> predicate, params Expression<Func<T, object>>[] includes)
        {
            var query = _entities.AsQueryable();
            return includes.Aggregate(query, (q, w) => q.Include(w)).Where(predicate).ToList();
        }

        public IList<T> GetAllInclude(params Expression<Func<T, object>>[] includes)
        {
            var query = _entities.AsQueryable();
            return includes.Aggregate(query, (q, w) => q.Include(w)).ToList();
        }

        public bool Exists(Func<T, bool> predicate)
        {
            return _entities.Any(predicate);
        }

        public int Count()
        {
            return _entities.Count();
        }

        public int Count(Func<T, bool> predicate)
        {
            return _entities.Count(predicate);
        }
    }
}
