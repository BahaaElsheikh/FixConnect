using FixConnect.DAL.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FixConnect.DAL.Repositories
{
    public class GenericRepository<T> where T : class
    {
        // ✅ DI: AppDbContext is injected here via constructor
        protected readonly AppDbContext Context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            Context = context;
            _dbSet = context.Set<T>();
        }

        public IEnumerable<T> GetAll()
            => _dbSet.ToList();

        public T? GetById(int id)
            => _dbSet.Find(id);

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
            => _dbSet.Where(predicate).ToList();

        public void Add(T entity)
        {
            _dbSet.Add(entity);
            Context.SaveChanges();
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
            Context.SaveChanges();
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
            Context.SaveChanges();
        }

        public bool Exists(Expression<Func<T, bool>> predicate)
            => _dbSet.Any(predicate);
    }
}