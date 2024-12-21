using Domain.Common;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Infrastructure.Interfaces
{
    public interface IRepository<T> where T : Base
    {
        Task<int> AddAsync(T entity);
        Task<bool> DeleteAsync(T entity);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIDAsync(int id);
        Task<bool> UpdateAsync(T entity);
        Task<List<T>> GetByConditionAsync(Expression<Func<T, bool>> predicate);
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
