using Domain.Common;
using Domain.Users;
using Infrastructure.DataContext;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataRepositories
{
    public class Repository<T> : IRepository<T> where T : Base
    {
        private readonly DomainDataContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(DomainDataContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }
        public int Add(T entity)
        {
            _dbSet.Add(entity);
            if (_context.SaveChanges() != 1) return -1;
            return entity.Id;
        }

        public bool Delete(T entity)
        {
            _dbSet.Remove(entity);
            return _context.SaveChanges() == 1;
        }

        public IEnumerable<T> GetAll()
        {
            return [.. _dbSet];
        }

        public T? GetByID(int id)
        {
            return _dbSet.Find(id);
        }

        public bool Update(T entity)
        {
            _dbSet.Update(entity);
            return _context.SaveChanges() == 1;
        }
    }
}
